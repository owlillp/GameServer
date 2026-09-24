using AuthService.Core.Abstractions;
using AuthService.Core.Features.Auth.Services;
using CSharpFunctionalExtensions;
using Shared.Core.Abstractions;
using Shared.Core.Database;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Features.Auth.Commands.JwtLogout;

public sealed class JwtLogoutHandler(
    IRefreshSessionRepository refreshSessionRepository,
    IRefreshTokenService refreshTokenService,
    ITransactionManager transactionManager,
    TimeProvider timeProvider) : ICommandHandler<JwtLogoutCommand>
{
    public async Task<UnitResult<Error>> Handle(
        JwtLogoutCommand command,
        CancellationToken cancellationToken = new ())
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return UnitResult.Success<Error>();
        }

        var transactionResult = await transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error;
        }

        string tokenHash = refreshTokenService.Hash(command.RefreshToken);
        var sessionResult = await refreshSessionRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
        if (sessionResult.IsFailure)
        {
            return sessionResult.Error.Type == ErrorType.NOT_FOUND
                ? UnitResult.Success<Error>()
                : sessionResult.Error;
        }

        sessionResult.Value.Revoke(timeProvider.GetUtcNow().UtcDateTime);
        return await transactionManager.CommitTransactionAsync(cancellationToken);
    }
}