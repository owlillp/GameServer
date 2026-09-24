using AuthService.Contracts.Responses;
using AuthService.Core.Abstractions;
using AuthService.Core.Configurations;
using AuthService.Core.Features.Auth.Services;
using AuthService.Domain;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Shared.Core.Abstractions;
using Shared.Core.Database;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Features.Auth.Commands.JwtRefresh;

public sealed class JwtRefreshHandler(
    IRefreshSessionRepository refreshSessionsRepository,
    IRefreshTokenService refreshTokenService,
    IJwtTokenService jwtTokenService,
    UserManager<Account> userManager,
    ITransactionManager transactionManager,
    IOptions<JwtSettings> options,
    TimeProvider timeProvider) : ICommandHandler<JwtRefreshResponse, JwtRefreshCommand>
{
    private readonly JwtSettings _settings = options.Value;

    public async Task<Result<JwtRefreshResponse, Error>> Handle(
        JwtRefreshCommand command,
        CancellationToken cancellationToken = new ())
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return RefreshSessionErrors.Invalid();
        }

        var transactionResult = await transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error;
        }

        string currentHash = refreshTokenService.Hash(command.RefreshToken);
        var sessionResult = await refreshSessionsRepository.GetByTokenHashAsync(currentHash, cancellationToken);
        if (sessionResult.IsFailure)
        {
            return sessionResult.Error.Type == ErrorType.NOT_FOUND
                ? RefreshSessionErrors.Invalid()
                : sessionResult.Error;
        }

        var session = sessionResult.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        if (session.IsExpired(now))
        {
            session.Revoke(now);
            var expiredCommit = await transactionManager.CommitTransactionAsync(cancellationToken);
            return expiredCommit.IsFailure
                ? expiredCommit.Error
                : RefreshSessionErrors.Invalid();
        }

        if (session.IsRevoked)
        {
            var chainResult = await refreshSessionsRepository.GetByAccountIdAsync(session.AccountId, cancellationToken);
            if (chainResult.IsFailure)
            {
                return chainResult.Error;
            }

            RevokeReplacementChain(session, chainResult.Value, now);
            var reusedCommit = await transactionManager.CommitTransactionAsync(cancellationToken);
            return reusedCommit.IsFailure
                ? reusedCommit.Error
                : RefreshSessionErrors.Invalid();
        }

        string replacementToken = refreshTokenService.GenerateToken();
        var replacementExpiresAt = now.AddDays(_settings.RefreshTokenLifetimeDays);
        var replacementSession = RefreshSession.Create(
            session.AccountId,
            refreshTokenService.Hash(replacementToken),
            now,
            replacementExpiresAt);

        session.RotateTo(replacementSession, now);
        await refreshSessionsRepository.AddAsync(replacementSession, cancellationToken);

        var commitResult = await transactionManager.CommitTransactionAsync(cancellationToken);
        if (commitResult.IsFailure)
        {
            return commitResult.Error;
        }

        var roles = await userManager.GetRolesAsync(session.Account);
        var accessToken = jwtTokenService.Generate(session.Account, [.. roles]);

        return new JwtRefreshResponse(
            accessToken.AccessToken,
            accessToken.ExpiresAt,
            replacementToken,
            replacementExpiresAt);
    }

    private static void RevokeReplacementChain(
        RefreshSession first,
        IReadOnlyList<RefreshSession> sessions,
        DateTime revokedAt)
    {
        var byId = sessions.ToDictionary(s => s.Id);
        var current = first;

        while (current is not null)
        {
            current.Revoke(revokedAt);
            current = current.ReplacedById is { } nextId && byId.TryGetValue(nextId, out RefreshSession? next)
                ? next
                : null;
        }
    }
}