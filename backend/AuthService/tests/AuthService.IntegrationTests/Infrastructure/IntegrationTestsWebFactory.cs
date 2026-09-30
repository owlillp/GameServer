using AuthService.Core.Features.Connect;
using AuthService.Infrastructure.Postgres;
using AuthService.Web.Configurations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenIddict.Server.AspNetCore;
using Respawn;
using System.Threading.RateLimiting;
using Testcontainers.PostgreSql;

namespace AuthService.IntegrationTests.Infrastructure;

public sealed class IntegrationTestsWebFactory : WebApplicationFactory<AuthService.Web.Program>, IAsyncLifetime
{
    public const string WEB_CLIENT_ID = "gameserver-web-tests";
    public const string WEB_REDIRECT_URI = "http://localhost/auth/callback";
    public const string GAME_CLIENT_ID = "gameserver-unity-tests";
    public const string GAME_REDIRECT_URI = "http://127.0.0.1:7777/callback";
    public const string GAME_CUSTOM_SCHEME_REDIRECT_URI = "gameserver://auth/callback";
    public const string SERVICE_CLIENT_ID = "auth-service-client-tests";
    public const string SERVICE_CLIENT_SECRET = "test-service-client-secret-0123456789";
    public const string FRONTEND_LOGIN_URL = "http://localhost/login";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("auth_service_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private NpgsqlConnection _connection = null!;
    private Respawner _respawner = null!;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        ConnectionString = _database.GetConnectionString();

        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        AuthServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<AuthServiceDbContext>();
        await dbContext.Database.MigrateAsync();

        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["auth"],
        });
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        await _database.DisposeAsync();
    }

    public Task ResetDatabaseAsync() => _respawner.ResetAsync(_connection);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // UseSetting (unlike ConfigureAppConfiguration) is applied before Program
        // eagerly reads configuration during DI registration.
        builder.UseSetting("ConnectionStrings:AuthDb", ConnectionString);
        builder.UseSetting("OpenIddict:Issuer", "http://localhost/");
        builder.UseSetting("OpenIddict:AccessTokenLifetimeMinutes", "5");
        builder.UseSetting("OpenIddict:RefreshTokenLifetimeDays", "7");
        builder.UseSetting("OpenIddict:Clients:Web:ClientId", WEB_CLIENT_ID);
        builder.UseSetting("OpenIddict:Clients:Web:DisplayName", "Test Web Client");
        builder.UseSetting("OpenIddict:Clients:Web:RedirectUris:0", WEB_REDIRECT_URI);
        builder.UseSetting("OpenIddict:Clients:Web:PostLogoutRedirectUris:0", "http://localhost/");
        builder.UseSetting("OpenIddict:Clients:Game:ClientId", GAME_CLIENT_ID);
        builder.UseSetting("OpenIddict:Clients:Game:DisplayName", "Test Unity Client");
        builder.UseSetting("OpenIddict:Clients:Game:RedirectUris:0", GAME_REDIRECT_URI);
        builder.UseSetting("OpenIddict:Clients:Game:RedirectUris:1", GAME_CUSTOM_SCHEME_REDIRECT_URI);
        builder.UseSetting("OpenIddict:Clients:Service:ClientId", SERVICE_CLIENT_ID);
        builder.UseSetting("OpenIddict:Clients:Service:ClientSecret", SERVICE_CLIENT_SECRET);
        builder.UseSetting("OpenIddict:Clients:Service:DisplayName", "Test Service Client");
        builder.UseSetting("AuthService:FrontendLoginUrl", FRONTEND_LOGIN_URL);

        builder.ConfigureTestServices(services =>
        {
            RemoveHostedService<OpenIddictSeeder>(services);

            // Rate limiter не должен влиять на тесты: политика без лимита.
            services.RemoveAll<IConfigureOptions<RateLimiterOptions>>();
            services.Configure<RateLimiterOptions>(options =>
                options.AddPolicy(
                    ConnectConstants.TOKEN_RATE_LIMIT_POLICY,
                    _ => RateLimitPartition.GetNoLimiter(string.Empty)));

            services.Configure<OpenIddictServerAspNetCoreOptions>(options =>
                options.DisableTransportSecurityRequirement = true);
        });
    }

    private static void RemoveHostedService<TImplementation>(IServiceCollection services)
    {
        foreach (ServiceDescriptor descriptor in services
                     .Where(descriptor => descriptor.ImplementationType == typeof(TImplementation))
                     .ToList())
        {
            services.Remove(descriptor);
        }
    }
}
