using AuthService.Contracts.HttpCommunication;
using ClanService.Core;
using ClanService.Core.Authorization;
using ClanService.Infrastructure.Postgres;
using Shared.Framework.Authentication;
using Shared.Framework.Cors;
using Shared.Framework.Endpoints;
using Shared.Framework.OpenApi;

namespace ClanService.Web.Configurations;

public static class DependencyInjectionExtensions
{
    private const string POSTGRESQL_HEALTH_CHECK = "postgresql";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddDependency(
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            // JWT-валидация токенов AuthService + permission-инфраструктура Shared.
            services.AddPlatformAuthentication(
                configuration,
                ClanRolePermissions.Map,
                sectionName: "Authentication");

            services.AddRateLimiter(_ => { });
            services.AddFrameworkCors(configuration);
            services.AddOpenApiSpec(Constants.SERVICE_NAME, "v1");
            services.AddHealthChecks().AddDbContextCheck<ClanServiceDbContext>(POSTGRESQL_HEALTH_CHECK);

            services.AddEndpoints(typeof(DependencyInjectionExtensions).Assembly);

            services.AddCore();
            services.AddInfrastructurePostgres(configuration);
            services.AddAuthServiceHttpCommunication(configuration);

            return services;
        }
    }
}
