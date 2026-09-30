using System.Security.Claims;
using AuthService.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Shared.Framework.Endpoints;

namespace AuthService.Core.Features.Connect;

public sealed class UserInfoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet($"/{ConnectConstants.USER_INFO_ENDPOINT}", HandleAsync).AllowAnonymous();
        app.MapPost($"/{ConnectConstants.USER_INFO_ENDPOINT}", HandleAsync).AllowAnonymous();
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        UserManager<Account> userManager)
    {
        AuthenticateResult authResult = await httpContext.AuthenticateAsync(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        ClaimsPrincipal? principal = authResult.Principal;
        if (!authResult.Succeeded || principal is null)
        {
            return Results.Challenge(
                authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }

        string? userId = principal.GetClaim(OpenIddictConstants.Claims.Subject);
        Account? user = userId is not null
            ? await userManager.FindByIdAsync(userId)
            : null;

        if (user is null)
        {
            return Results.Challenge(
                authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }

        IList<string> roles = await userManager.GetRolesAsync(user);

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [OpenIddictConstants.Claims.Subject] = user.Id.ToString(),
            [OpenIddictConstants.Claims.Name] = user.DisplayName ?? user.UserName ?? string.Empty,
            [OpenIddictConstants.Claims.Email] = user.Email ?? string.Empty,
            [OpenIddictConstants.Claims.PreferredUsername] = user.UserName ?? string.Empty,
            [OpenIddictConstants.Claims.Role] = roles,
        };

        return Results.Ok(claims);
    }
}
