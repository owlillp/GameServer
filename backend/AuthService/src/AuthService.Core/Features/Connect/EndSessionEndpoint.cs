using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Server.AspNetCore;
using Shared.Framework.Endpoints;

namespace AuthService.Core.Features.Connect;

public sealed class EndSessionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet($"/{ConnectConstants.END_SESSION_ENDPOINT}", HandleAsync).AllowAnonymous();
        app.MapPost($"/{ConnectConstants.END_SESSION_ENDPOINT}", HandleAsync).AllowAnonymous();
    }

    private static async Task<IResult> HandleAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await httpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        return Results.SignOut(
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }
}
