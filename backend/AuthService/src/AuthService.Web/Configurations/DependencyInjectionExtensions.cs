using AuthService.Core;
using AuthService.Infrastructure.Postgres;
using Shared.Framework.Authentication;
using Shared.Framework.Cors;
using Shared.Framework.Endpoints;
using Shared.Framework.OpenApi;

namespace AuthService.Web.Configurations;

public static class DependencyInjectionExtensions
{
    private const string POSTGRESQL_HEALTH_CHECK = "postgresql";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddDependency(IConfiguration configuration)
        {
            services.AddAuthorization();
            services.AddCurrentUser();

            services.AddFrameworkCors(configuration);
            services.AddOpenApiSpec(Constants.SERVICE_NAME, "v1");
            services.AddHealthChecks().AddDbContextCheck<AuthServiceDbContext>(POSTGRESQL_HEALTH_CHECK);
            services.AddEndpoints(typeof(DependencyInjectionExtensions).Assembly);

            services.AddCore(configuration);
            services.AddInfrastructurePostgres(configuration);

            return services;
        }
    }
}