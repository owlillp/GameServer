using AuthService.Contracts.Responses;
using AuthService.Core.Abstractions;
using AuthService.Core.Features.Auth.Services;
using CSharpFunctionalExtensions;
using Shared.Core.Abstractions;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Features.Auth.Queries.GetJwtSesionStatus;

public sealed class JwtSessionStatusHandler(
    IRefreshSessionRepository refreshSessionRepository,
    IRefreshTokenService refreshTokenService,
    TimeProvider timeProvider) : IQueryHandlerWithResult<JwtSessionStatusResponse, JwtSessionStatusQuery>
{
    public async Task<Result<JwtSessionStatusResponse, Error>> Handle(
        JwtSessionStatusQuery query,
        CancellationToken cancellationToken = new ())
    {
        if (string.IsNullOrWhiteSpace(query.RefreshToken))
        {
            return new JwtSessionStatusResponse(
                HasRefreshCookie: false,
                IsRefreshSessionActive: false,
                RefreshSessionExpiresAt: null,
                RefreshSessionRevokedAt: null);
        }

        string tokenHash = refreshTokenService.Hash(query.RefreshToken);
        var sessionResult = await refreshSessionRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
        if (sessionResult.IsFailure)
        {
            return sessionResult.Error.Type == ErrorType.NOT_FOUND
                ? new JwtSessionStatusResponse(
                    HasRefreshCookie: true,
                    IsRefreshSessionActive: false,
                    RefreshSessionExpiresAt: null,
                    RefreshSessionRevokedAt: null)
                : sessionResult.Error;
        }

        var session = sessionResult.Value;
        bool isActive = !session.IsRevoked && !session.IsExpired(timeProvider.GetUtcNow().UtcDateTime);

        return new JwtSessionStatusResponse(
            HasRefreshCookie: true,
            IsRefreshSessionActive: isActive,
            RefreshSessionExpiresAt: session.ExpiresAt,
            RefreshSessionRevokedAt: session.RevokedAt);
    }
}