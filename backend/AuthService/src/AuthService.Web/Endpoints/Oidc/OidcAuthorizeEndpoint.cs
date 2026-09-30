using System.Security.Claims;
using AuthService.Core.Configurations;
using AuthService.Domain;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Shared.Framework.Authentication;
using Shared.Framework.Endpoints;

namespace AuthService.Web.Endpoints.Oidc;

public sealed class OidcAuthorizeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/" + ConnectConstants.AUTHORIZE_ENDPOINT, HandleAsync).AllowAnonymous();
        app.MapPost("/" + ConnectConstants.AUTHORIZE_ENDPOINT, HandleAsync).AllowAnonymous();
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        UserManager<Account> userManager,
        IOpenIddictScopeManager scopeManager,
        IOptions<ConnectOptions> connectOptions)
    {
        var request = httpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("OpenID Connect request cannot be retrieved.");

        var authResult = await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (authResult.Principal?.Identity?.IsAuthenticated != true)
        {
            string? loginUrl = connectOptions.Value.LoginUrl;
            if(string.IsNullOrEmpty(loginUrl))
                return Results.Unauthorized();

            string returnUrl = httpContext.Request.GetEncodedUrl();
            return Results.Redirect($"{loginUrl}?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        string userId = userManager.GetUserId(authResult.Principal)!;
        var user = (await userManager.FindByIdAsync(userId))!;
        var roles = await userManager.GetRolesAsync(user);

        var identity = new ClaimsIdentity(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        identity.SetClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());
        identity.SetClaim(OpenIddictConstants.Claims.Name, user.UserName);
        identity.SetClaim(OpenIddictConstants.Claims.Email, user.Email);
        identity.SetClaim(OpenIddictConstants.Claims.PreferredUsername, user.UserName);

        foreach (string role in roles)
        {
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Role, role));
        }

        identity.SetScopes(request.GetScopes());
        identity.SetResources(await scopeManager.ListResourcesAsync(request.GetScopes()).ToListAsync());
        identity.SetDestinations(GetDestinations);

        return Results.SignIn(
            new ClaimsPrincipal(identity),
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static IEnumerable<string> GetDestinations(Claim claim) => claim.Type switch
    {
        OpenIddictConstants.Claims.Subject
            or OpenIddictConstants.Claims.Name
            or OpenIddictConstants.Claims.Email
            or OpenIddictConstants.Claims.PreferredUsername
            or OpenIddictConstants.Claims.Role
            => [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken],
        _ => [OpenIddictConstants.Destinations.AccessToken],
    };
}
