using System.Security.Claims;
using AuthService.Core.Configurations;
using AuthService.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Shared.Framework.Endpoints;

namespace AuthService.Core.Features.Connect;

public sealed class AuthorizationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet($"/{ConnectConstants.AUTHORIZE_ENDPOINT}", HandleAsync).AllowAnonymous();
        app.MapPost($"/{ConnectConstants.AUTHORIZE_ENDPOINT}", HandleAsync).AllowAnonymous();
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        SignInManager<Account> signInManager,
        IOptions<AuthServiceOptions> authServiceOptions,
        IOpenIddictScopeManager scopeManager)
    {
        OpenIddictRequest request = httpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        AuthenticateResult authResult = await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        ClaimsPrincipal? principal = authResult.Principal;

        if (principal?.Identity?.IsAuthenticated != true)
        {
            return RedirectToLogin(httpContext, authServiceOptions.Value);
        }

        Account? user = await signInManager.ValidateSecurityStampAsync(principal);
        if (user is null
            || !await signInManager.CanSignInAsync(user)
            || await signInManager.UserManager.IsLockedOutAsync(user))
        {
            await httpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToLogin(httpContext, authServiceOptions.Value);
        }

        IList<string> roles = await signInManager.UserManager.GetRolesAsync(user);

        ClaimsIdentity identity = ConnectClaims.CreateUserIdentity(user, roles);
        identity.SetScopes(request.GetScopes());

        List<string> resources = await scopeManager
            .ListResourcesAsync(identity.GetScopes(), httpContext.RequestAborted)
            .ToListAsync(httpContext.RequestAborted);

        identity.SetResources(resources);
        identity.SetDestinations(ConnectClaims.GetDestinations);

        return Results.SignIn(
            new ClaimsPrincipal(identity),
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static IResult RedirectToLogin(HttpContext httpContext, AuthServiceOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.FrontendLoginUrl))
        {
            return Results.Challenge(authenticationSchemes: [IdentityConstants.ApplicationScheme]);
        }

        string returnUrl = httpContext.Request.GetEncodedUrl();
        string separator = options.FrontendLoginUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?";

        return Results.Redirect(
            $"{options.FrontendLoginUrl}{separator}returnUrl={Uri.EscapeDataString(returnUrl)}");
    }
}
