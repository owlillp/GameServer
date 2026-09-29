using Scalar.AspNetCore;
using Shared.Framework.Authentication;
using Shared.Framework.Cors;
using Shared.Framework.Endpoints;
using Shared.Framework.Logging;
using Shared.Framework.Middlewares;

namespace AuthService.Web.Configurations;

public static class AppConfigurationExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app, string[] args)
    {
        app.UseRouting();
        app.UseRateLimiter();
        app.UseSerilogHttpRequestLogging();
        app.UseExceptionMiddleware();
        app.UseRequestCorrelationId();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCurrentUser();

        if (!app.Environment.IsProduction())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.ConfigureCors();
        app.MapHealthChecks("/health");
        app.MapEndpoints();

        return app;
    }
}