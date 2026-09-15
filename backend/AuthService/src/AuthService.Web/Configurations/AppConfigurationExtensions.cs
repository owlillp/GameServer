using Scalar.AspNetCore;
using Shared.Framework.Cors;
using Shared.Framework.Endpoints;
using Shared.Framework.Logging;
using Shared.Framework.Middlewares;

namespace AuthService.Web.Configurations;

public static class AppConfigurationExtensions
{
    public static WebApplication Configure(this WebApplication app, string[] args)
    {
        app.UseSerilogHttpRequestLogging();
        app.UseExceptionMiddleware();
        app.ConfigureCors();

        app.UseAuthentication();
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