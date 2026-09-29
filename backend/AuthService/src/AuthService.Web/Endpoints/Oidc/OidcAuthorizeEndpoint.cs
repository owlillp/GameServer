using System.Security.Claims;
using AuthService.Core.Configurations;
using AuthService.Domain;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
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
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/connect/authorize", HandleAsync);

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        UserManager<Account> userManager,
        IOptions<OidcSettings> settings)
    {
        var request = httpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict authorization request is missing");

        var auth = await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!auth.Succeeded)
        {
            string returnUrl = httpContext.Request.PathBase + httpContext.Request.Path + httpContext.Request.QueryString;
            string frontendLoginUrl = settings.Value.FrontendLoginUrl;

            return string.IsNullOrEmpty(frontendLoginUrl)
                ? Results.Unauthorized()
                : Results.Redirect(QueryHelpers.AddQueryString(frontendLoginUrl, "returnUrl", returnUrl));
        }

        string? userId = auth.Principal.FindFirstValue(AuthClaimTypes.SUB)
            ?? auth.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        Account? account = Guid.TryParse(userId, out Guid id)
            ? await userManager.FindByIdAsync(id.ToString())
            : null;

        if (account is null)
        {
            return Forbid(OpenIddictConstants.Errors.AccessDenied, "Пользователь не найден.");
        }

        IList<string> roles = await userManager.GetRolesAsync(account);

        var identity = new ClaimsIdentity(
            authenticationType: "OpenIddict",
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        identity.SetClaim(OpenIddictConstants.Claims.Subject, account.Id.ToString());
        identity.SetClaim(OpenIddictConstants.Claims.Name, account.DisplayName ?? account.UserName);
        identity.SetClaim(OpenIddictConstants.Claims.PreferredUsername, account.UserName);
        identity.SetClaim(OpenIddictConstants.Claims.Email, account.Email);

        foreach (string role in roles)
        {
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Role, role));
        }

        identity.SetScopes(request.GetScopes());
        identity.SetResources(OidcScopes.GetResources(request.GetScopes()));
        identity.SetDestinations(GetDestinations);

        return Results.SignIn(
            new ClaimsPrincipal(identity),
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        ClaimsIdentity identity = claim.Subject!;

        if (claim.Type is OpenIddictConstants.Claims.Subject)
        {
            yield return OpenIddictConstants.Destinations.AccessToken;
            yield return OpenIddictConstants.Destinations.IdentityToken;
            yield break;
        }

        if (claim.Type is OpenIddictConstants.Claims.Name or OpenIddictConstants.Claims.PreferredUsername
            && identity.HasScope(OidcScopes.PROFILE))
        {
            yield return OpenIddictConstants.Destinations.IdentityToken;
            yield break;
        }

        if (claim.Type is OpenIddictConstants.Claims.Email && identity.HasScope(OidcScopes.EMAIL))
        {
            yield return OpenIddictConstants.Destinations.AccessToken;
            yield return OpenIddictConstants.Destinations.IdentityToken;
            yield break;
        }

        if (claim.Type is OpenIddictConstants.Claims.Role)
        {
            yield return OpenIddictConstants.Destinations.AccessToken;
        }
    }

    private static IResult Forbid(string error, string description) =>
        Results.Forbid(
            new AuthenticationProperties(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description,
            }),
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
}
