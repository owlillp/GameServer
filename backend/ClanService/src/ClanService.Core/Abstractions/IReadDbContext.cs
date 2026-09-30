using ClanService.Domain;

namespace ClanService.Core.Abstractions;

public interface IReadDbContext
{
    IQueryable<Clan> ClansRead { get; }
}
