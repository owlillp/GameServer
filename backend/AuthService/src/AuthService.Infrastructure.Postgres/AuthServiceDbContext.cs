using AuthService.Core.Abstractions;
using AuthService.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Postgres;

public class AuthServiceDbContext(DbContextOptions<AuthServiceDbContext> options)
    : IdentityDbContext<Account, Role, Guid>(options), IReadDbContext
{
    public IQueryable<Account> AccountsRead => Set<Account>()
        .AsQueryable()
        .AsNoTracking();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(Constants.SCHEMA);
        builder.ApplyConfigurationsFromAssembly(typeof(AuthServiceDbContext).Assembly);
    }
}