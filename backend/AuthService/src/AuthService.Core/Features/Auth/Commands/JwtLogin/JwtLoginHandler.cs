using AuthService.Core.Abstractions;
using AuthService.Core.Configurations;
using AuthService.Core.Features.Auth.Services;
using AuthService.Domain;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Shared.Core.Abstractions;
using Shared.Core.Database;
using Shared.Core.Validation;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Features.Auth.Commands.JwtLogin;

public sealed class JwtLoginHandler(
    IValidator<JwtLoginCommand> validator,
    ITransactionManager transactionManager,
    SignInManager<Account> signInManager,
    UserManager<Account> userManager,
    IRefreshTokenService refreshTokenService,
    IRefreshSessionRepository refreshSessionRepository,
    IOptions<JwtSettings> options,
    TimeProvider timeProvider,
    IJwtTokenService jwtTokenService)
    : ICommandHandler<JwtLoginResult, JwtLoginCommand>
{
    private readonly JwtSettings _settings = options.Value;

    public async Task<Result<JwtLoginResult, Error>> Handle(
        JwtLoginCommand command,
        CancellationToken cancellationToken = new ())
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var transactionResult = await transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error;
        }

        Account? account = await userManager.FindByEmailAsync(command.Request.Email);
        if (account is null)
        {
            return InvalidCredentials();
        }

        var checkResult = await signInManager.CheckPasswordSignInAsync(
            account,
            command.Request.Password,
            lockoutOnFailure: true);

        if (!checkResult.Succeeded)
        {
            return MapSignInFailure(checkResult);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var refreshExpiresAt = now.AddDays(_settings.RefreshTokenLifetimeDays);
        string refreshToken = refreshTokenService.GenerateToken();
        var refreshSession = RefreshSession.Create(
            account.Id,
            refreshTokenService.Hash(refreshToken),
            now,
            refreshExpiresAt);

        await refreshSessionRepository.AddAsync(refreshSession, cancellationToken);

        var commitResult = await transactionManager.CommitTransactionAsync(cancellationToken);
        if (commitResult.IsFailure)
        {
            return commitResult.Error;
        }

        var roles = await userManager.GetRolesAsync(account);
        var token = jwtTokenService.Generate(account, [.. roles]);

        return new JwtLoginResult(
            token.AccessToken,
            token.ExpiresAt,
            refreshToken,
            refreshExpiresAt);
    }

    private static Error InvalidCredentials() =>
        Error.Authentication("invalid.credentials", "Invalid email or password");

    private static Error MapSignInFailure(SignInResult result)
    {
        if (result.IsLockedOut)
        {
            return Error.Authentication(
                "account.locked",
                "Аккаунт временно заблокирован из-за множества неудачных попыток входа");
        }

        if (result.IsNotAllowed)
        {
            return Error.Authentication(
                "account.not.allowed",
                "Вход не разрешён (требуется подтверждение email/телефона)");
        }

        if (result.RequiresTwoFactor)
        {
            return Error.Authentication(
                "two.factor.required",
                "Требуется двухфакторная аутентификация");
        }

        return InvalidCredentials();
    }
}