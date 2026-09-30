using AuthService.Contracts.Admin;
using AuthService.Core.Abstractions;
using AuthService.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Abstractions;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Features.Admin.Queries.GetStats;

public sealed class GetStatsHandler(IReadDbContext readDbContext)
    : IQueryHandlerWithResult<AdminStatsDto, GetStatsQuery>
{
    public async Task<Result<AdminStatsDto, Error>> Handle(
        GetStatsQuery query,
        CancellationToken cancellationToken = new ())
    {
        int totalUsers = await readDbContext.AccountsRead.CountAsync(cancellationToken);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        int lockedOutCount = await readDbContext.AccountsRead
            .CountAsync(account => account.LockoutEnd != null && account.LockoutEnd > now, cancellationToken);

        Dictionary<string, int> roleCounts = await GetRoleCountsAsync(cancellationToken);

        return new AdminStatsDto(
            totalUsers,
            roleCounts.GetValueOrDefault(AuthRoles.ADMIN),
            roleCounts.GetValueOrDefault(AuthRoles.MODERATOR),
            lockedOutCount);
    }

    private async Task<Dictionary<string, int>> GetRoleCountsAsync(CancellationToken cancellationToken)
    {
        var rows = await (
                from accountRole in readDbContext.AccountRolesRead
                join role in readDbContext.RolesRead on accountRole.RoleId equals role.Id
                group accountRole by role.Name
                into roleGroup
                select new { RoleName = roleGroup.Key!, Count = roleGroup.Count() })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(row => row.RoleName, row => row.Count, StringComparer.Ordinal);
    }
}
