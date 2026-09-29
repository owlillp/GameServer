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
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/connect/userinfo", HandleAsync);

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        UserManager<Account> userManager)
    {
        var auth = await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (!auth.Succeeded || auth.Principal is null)
        {
            return Challenge();
        }

        ClaimsPrincipal principal = auth.Principal;
        string? subject = principal.GetClaim(OpenIddictConstants.Claims.Subject);

        Account? account = Guid.TryParse(subject, out Guid id)
            ? await userManager.FindByIdAsync(id.ToString())
            : null;

        if (account is null)
        {
            return Challenge();
        }

        var response = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["sub"] = account.Id.ToString(),
        };

        if (principal.HasScope(OidcScopes.PROFILE))
        {
            response["name"] = account.DisplayName ?? account.UserName;
            response["preferred_username"] = account.UserName;
        }

        if (principal.HasScope(OidcScopes.EMAIL))
        {
            response["email"] = account.Email;
        }

        return Results.Ok(response);
    }

    private static IResult Challenge() =>
        Results.Challenge(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
}
