using ClanService.Core.Abstractions;
using ClanService.Domain;
using Microsoft.EntityFrameworkCore;

namespace ClanService.Infrastructure.Postgres;

public class ClanServiceDbContext(DbContextOptions<ClanServiceDbContext> options)
    : DbContext(options), IReadDbContext
{
    public DbSet<Clan> Clans => Set<Clan>();

    public IQueryable<Clan> ClansRead => Set<Clan>()
        .AsQueryable()
        .AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Constants.SCHEMA);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClanServiceDbContext).Assembly);
    }
}
