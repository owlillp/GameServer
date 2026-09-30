using Scalar.AspNetCore;
using Shared.Framework.Authentication.UserScope;
using Shared.Framework.Cors;
using Shared.Framework.Endpoints;
using Shared.Framework.Logging;
using Shared.Framework.Middlewares;

namespace AuthService.Web.Configurations;

public static class AppConfigurationExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        app.UseRouting();
        app.ConfigureCors();
        app.UseRateLimiter();
        app.UseSerilogHttpRequestLogging();
        app.UseExceptionMiddleware();
        app.UseRequestCorrelationId();

        app.UseAuthentication();
        app.UseUserScopedData();
        app.UseAuthorization();

        if (!app.Environment.IsProduction())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.MapHealthChecks("/health");
        app.MapEndpoints();

        return app;
    }
}
