using AuthService.Core.Configurations;
using AuthService.Infrastructure.Postgres;
using AuthService.Infrastructure.Postgres.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthService.Web;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAndSeedAsync(this WebApplication app)
    {
        if (app.Environment.IsEnvironment("Testing"))
        {
            return;
        }

        await using AsyncServiceScope scope = app.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AuthServiceDbContext>();
        await dbContext.Database.MigrateAsync();

        var oidcSettings = scope.ServiceProvider
            .GetRequiredService<IOptions<OidcSettings>>()
            .Value;

        if (oidcSettings.Enabled)
        {
            var seeder = scope.ServiceProvider.GetRequiredService<OidcServerSeeder>();
            await seeder.SeedAsync();
        }

        app.Logger.LogInformation("Migrations applied and OpenIdDict seed completed");
    }
}