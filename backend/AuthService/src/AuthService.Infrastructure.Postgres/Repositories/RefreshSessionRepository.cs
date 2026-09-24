using AuthService.Core.Abstractions;
using AuthService.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel.Errors;

namespace AuthService.Infrastructure.Postgres.Repositories;

public sealed class RefreshSessionRepository(
    ILogger<RefreshSessionRepository> logger,
    AuthServiceDbContext dbContext) : IRefreshSessionRepository
{
    public async Task<Result<RefreshSession, Error>> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await dbContext.RefreshSessions
                .FromSql($"SELECT * FROM auth.refresh_sessions WHERE token_hash = {tokenHash} FOR UPDATE")
                .Include(s => s.Account)
                .SingleOrDefaultAsync(cancellationToken);

            return session is null
                ? RefreshSessionErrors.NotFound()
                : session;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation cancelled while loading refresh session");
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load refresh session");
            return GeneralErrors.DatabaseError();
        }
    }

    public async Task<Result<IReadOnlyList<RefreshSession>, Error>> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        try
        {
            var sessions = await dbContext.RefreshSessions
                .FromSql($"SELECT * FROM auth.refresh_sessions WHERE account_id = {accountId} FOR UPDATE")
                .ToListAsync(cancellationToken);

            return Result.Success<IReadOnlyList<RefreshSession>, Error>(sessions);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation cancelled while loading refresh session chain");
            return GeneralErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load refresh session chain");
            return GeneralErrors.DatabaseError();
        }
    }

    public async Task AddAsync(
        RefreshSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        await dbContext.RefreshSessions.AddAsync(session, cancellationToken);
    }
}