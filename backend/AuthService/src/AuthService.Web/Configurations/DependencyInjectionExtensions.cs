using AuthService.Core;
using AuthService.Core.Features.Connect;
using AuthService.Core.Services;
using AuthService.Domain;
using AuthService.Infrastructure.Postgres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Framework.Authentication;
using Shared.Framework.Authentication.UserScope;
using Shared.Framework.Authorization.Permissions;
using Shared.Framework.Authorization.Roles;
using Shared.Framework.Cors;
using Shared.Framework.Endpoints;
using Shared.Framework.OpenApi;
using System.Threading.RateLimiting;

namespace AuthService.Web.Configurations;

public static class DependencyInjectionExtensions
{
    private const string POSTGRESQL_HEALTH_CHECK = "postgresql";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddDependency(
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            services.AddAuthorization();
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // /connect/token: грубый потолок на IP (в т.ч. для password grant
                // Unity-клиента). Точечную защиту от перебора пароля даёт
                // Identity lockout на конкретный аккаунт.
                options.AddPolicy(ConnectConstants.TOKEN_RATE_LIMIT_POLICY, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 120,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                        }));
            });

            services.AddPermissionAuthorization();
            services.AddUserScopedData();

            services.AddFrameworkCors(configuration);
            services.AddOpenApiSpec(Constants.SERVICE_NAME, "v1");
            services.AddHealthChecks().AddDbContextCheck<AuthServiceDbContext>(POSTGRESQL_HEALTH_CHECK);

            services.AddEndpoints(typeof(DependencyInjectionExtensions).Assembly);
            services.AddEndpoints(typeof(AuthorizationEndpoint).Assembly);

            services.AddCore(configuration);
            services.AddInfrastructurePostgres(configuration);
            services.AddAuthServiceOpenIddict(configuration, environment);
            services.AddScoped<PlatformConfigSyncService>();

            return services;
        }

        private IServiceCollection AddPermissionAuthorization()
        {
            services.AddSingleton<IReadOnlyDictionary<string, IReadOnlyList<string>>>(RolePermissions.Map);
            services.AddSingleton<IPermissionResolver, RolePermissionResolver>();
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();

            return services;
        }
    }
}
