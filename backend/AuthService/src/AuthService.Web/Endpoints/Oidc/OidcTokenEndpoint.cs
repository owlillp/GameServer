using System.Security.Claims;
using AuthService.Domain;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Shared.Framework.Endpoints;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace AuthService.Web.Endpoints.Oidc;

public sealed class OidcTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app
            .MapPost("/" + ConnectConstants.TOKEN_ENDPOINT, HandleAsync)
            .AllowAnonymous();

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        UserManager<Account> userManager,
        IOpenIddictScopeManager scopeManager)
    {
        var request = httpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenID Connect request cannot be retrieved.");

        if (request.IsClientCredentialsGrantType())
        {
            return await HandleClientCredentialsAsync(request, scopeManager);
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            return await HandleCodeOrRefreshAsync(httpContext, userManager, scopeManager);
        }

        return Results.BadRequest(new { error = OpenIddictConstants.Errors.UnsupportedGrantType });
    }

    private static async Task<IResult> HandleClientCredentialsAsync(
        OpenIddictRequest request,
        IOpenIddictScopeManager scopeManager)
    {
        var identity = new ClaimsIdentity(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        identity.SetClaim(OpenIddictConstants.Claims.Subject, request.ClientId);
        identity.AddClaim(OpenIddictConstants.Claims.Role, PlatformRoles.SERVICE_ACCOUNT);

        identity.SetScopes(request.GetScopes());

        var resources = await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync();
        identity.SetResources(resources);

        identity.SetAudiences(resources);
        identity.SetDestinations(_ => [OpenIddictConstants.Destinations.AccessToken]);

        return Results.SignIn(
            new ClaimsPrincipal(identity),
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IResult> HandleCodeOrRefreshAsync(
        HttpContext httpContext,
        UserManager<Account> userManager,
        IOpenIddictScopeManager scopeManager)
    {
        var principal = (await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
        string? userId = principal?.GetClaim(OpenIddictConstants.Claims.Subject);
        var user = userId != null
            ? await userManager.FindByIdAsync(userId)
            : null;

        if (user is null)
        {
            return Results.Forbid(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }

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
            identity.AddClaim(OpenIddictConstants.Claims.Role, role);
        }

        identity.SetScopes(principal!.GetScopes());
        identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());
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
