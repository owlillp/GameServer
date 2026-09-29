using System.Security.Claims;
using AuthService.Core.Configurations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Shared.Framework.Endpoints;

namespace AuthService.Web.Endpoints.Oidc;

public sealed class OidcTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/connect/token", (Func<HttpContext, Task<IResult>>)HandleAsync);

    private static async Task<IResult> HandleAsync(HttpContext httpContext)
    {
        var request = httpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIdDict token request is missing");

        if (request.IsClientCredentialsGrantType())
        {
            var identity = new ClaimsIdentity(
                authenticationType: "OpenIddict",
                nameType: OpenIddictConstants.Claims.Name,
                roleType: OpenIddictConstants.Claims.Role);

            identity.SetClaim(OpenIddictConstants.Claims.Subject, request.ClientId);
            identity.SetScopes(request.GetScopes());
            identity.SetResources(OidcScopes.GetResources(request.GetScopes()));
            identity.SetDestinations(static _ => [OpenIddictConstants.Destinations.AccessToken]);

            return Results.SignIn(
                new ClaimsPrincipal(identity),
                authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var auth = await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (!auth.Succeeded || auth.Principal is null)
            {
                return Results.Forbid(
                    Error(OpenIddictConstants.Errors.InvalidGrant, "The authorization code or refresh token is invalid."),
                    [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
            }

            return Results.SignIn(
                auth.Principal,
                authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return Results.Forbid(
            Error(OpenIddictConstants.Errors.UnsupportedGrantType, "The specified grant type is not supported."),
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }

    private static AuthenticationProperties Error(string error, string description) =>
        new(new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description,
        });
}
