using ClanService.Domain;

namespace ClanService.Core.Abstractions;

public interface IClanRepository
{
    Task AddAsync(Clan clan, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task<bool> NameOrTagExistsAsync(string name, string tag, CancellationToken cancellationToken);

    Task<Clan?> GetByIdAsync(Guid clanId, CancellationToken cancellationToken);

    void Remove(Clan clan);
}
