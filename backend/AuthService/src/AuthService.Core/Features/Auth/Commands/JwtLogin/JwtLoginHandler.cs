using AuthService.Contracts.Responses;
using AuthService.Core.Features.Auth.Services;
using AuthService.Domain;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
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
    IJwtTokenService jwtTokenService)
    : ICommandHandler<JwtLoginResponse, JwtLoginCommand>
{
    public async Task<Result<JwtLoginResponse, Error>> Handle(
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

        var commitResult = await transactionManager.CommitTransactionAsync(cancellationToken);
        if (commitResult.IsFailure)
        {
            return commitResult.Error;
        }

        var roles = await userManager.GetRolesAsync(account);
        var token = jwtTokenService.Generate(account, [.. roles]);

        return new JwtLoginResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAt = token.ExpiresAt
        };
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