using AuthService.Contracts.HttpCommunication;
using ClanService.Infrastructure.Postgres;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace ClanService.IntegrationTests.Infrastructure;

public sealed class IntegrationTestsWebFactory : WebApplicationFactory<ClanService.Web.Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("clan_service_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private NpgsqlConnection _connection = null!;
    private Respawner _respawner = null!;

    public FakeAuthServiceClient AuthServiceClient { get; } = new();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        ConnectionString = _database.GetConnectionString();

        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        ClanServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<ClanServiceDbContext>();
        await dbContext.Database.MigrateAsync();

        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = [Constants.SCHEMA],
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

        builder.UseSetting("ConnectionStrings:ClanDb", ConnectionString);
        builder.UseSetting("Authentication:Authority", "http://localhost/");
        builder.UseSetting("Authentication:Audiences:0", "clan-service");
        builder.UseSetting("AuthServiceOptions:Url", "http://localhost/");
        builder.UseSetting("ServiceToken:TokenUrl", "http://localhost/connect/token");
        builder.UseSetting("ServiceToken:ClientId", "test-client");
        builder.UseSetting("ServiceToken:ClientSecret", "test-secret");

        builder.ConfigureTestServices(services =>
        {
            // Авторизация тестовой схемой вместо JWT AuthService.
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            // Вместо HTTP-вызова AuthService — управляемый фейк.
            services.RemoveAll<IAuthServiceClient>();
            services.AddSingleton(AuthServiceClient);
            services.AddSingleton<IAuthServiceClient>(sp => sp.GetRequiredService<FakeAuthServiceClient>());
        });
    }
}
