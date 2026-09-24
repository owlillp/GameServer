using AuthService.Domain;
using CSharpFunctionalExtensions;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Abstractions;

public interface IRefreshSessionRepository
{
    Task<Result<RefreshSession, Error>> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<RefreshSession>, Error>> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken);

    Task AddAsync(RefreshSession session, CancellationToken cancellationToken);
}
