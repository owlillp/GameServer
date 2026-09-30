using ClanService.Core.Abstractions;
using ClanService.Domain;
using Microsoft.EntityFrameworkCore;

namespace ClanService.Infrastructure.Postgres;

public sealed class ClanRepository(ClanServiceDbContext dbContext) : IClanRepository
{
    public Task AddAsync(Clan clan, CancellationToken cancellationToken)
    {
        dbContext.Clans.Add(clan);
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public Task<bool> NameOrTagExistsAsync(string name, string tag, CancellationToken cancellationToken) =>
        dbContext.Clans.AnyAsync(
            clan => clan.NormalizedName == name.ToUpperInvariant() || clan.Tag == tag,
            cancellationToken);

    public Task<Clan?> GetByIdAsync(Guid clanId, CancellationToken cancellationToken) =>
        dbContext.Clans
            .Include(clan => clan.Members)
            .FirstOrDefaultAsync(clan => clan.Id == clanId, cancellationToken);

    public void Remove(Clan clan) => dbContext.Clans.Remove(clan);
}
