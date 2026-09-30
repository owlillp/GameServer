using System.Security.Claims;
using AuthService.Core.Configurations;
using AuthService.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Shared.Framework.Endpoints;

namespace AuthService.Web.Endpoints.Oidc;

public sealed class OidcUserInfoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/" + ConnectConstants.USER_INFO_ENDPOINT, HandleAsync).AllowAnonymous();
        app.MapPost("/" + ConnectConstants.USER_INFO_ENDPOINT, HandleAsync).AllowAnonymous();
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        UserManager<Account> userManager)
    {
        var authResult = await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        ClaimsPrincipal? principal = authResult.Principal;
        if (!authResult.Succeeded || principal is null)
        {
            return Results.Challenge(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }

        string? userId = principal.GetClaim(OpenIddictConstants.Claims.Subject);
        var user = userId != null
            ? await userManager.FindByIdAsync(userId)
            : null;

        if (user is null)
        {
            return Results.Challenge(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = new Dictionary<string, object>()
        {
            [OpenIddictConstants.Claims.Subject] = user.Id.ToString(),
            [OpenIddictConstants.Claims.Name] = user.UserName!,
            [OpenIddictConstants.Claims.Email] = user.Email!,
            [OpenIddictConstants.Claims.PreferredUsername] = user.UserName!,
            [OpenIddictConstants.Claims.Role] = roles
        };

        return Results.Ok(claims);
    }
}
