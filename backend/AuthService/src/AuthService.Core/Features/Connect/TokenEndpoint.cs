using System.Security.Claims;
using AuthService.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Shared.Framework.Authentication;
using Shared.Framework.Endpoints;

namespace AuthService.Core.Features.Connect;

public sealed class TokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost($"/{ConnectConstants.TOKEN_ENDPOINT}", HandleAsync)
            .AllowAnonymous()
            .RequireRateLimiting(ConnectConstants.TOKEN_RATE_LIMIT_POLICY);

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        UserManager<Account> userManager,
        SignInManager<Account> signInManager,
        IOpenIddictScopeManager scopeManager)
    {
        OpenIddictRequest request = httpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsClientCredentialsGrantType())
        {
            return await HandleClientCredentialsAsync(request, scopeManager, httpContext.RequestAborted);
        }

        if (request.IsPasswordGrantType())
        {
            return await HandlePasswordGrantAsync(
                request,
                userManager,
                signInManager,
                scopeManager,
                httpContext.RequestAborted);
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            return await HandleUserGrantAsync(httpContext, request, userManager, scopeManager);
        }

        return Results.BadRequest(new { error = OpenIddictConstants.Errors.UnsupportedGrantType });
    }

    // In-game логин Unity-клиента: email/username + пароль -> токены.
    // Ошибаемся всегда invalid_grant, чтобы не подсказывать, что именно неверно;
    // перебор ограничивают Identity lockout и rate limiter на /connect/token.
    private static async Task<IResult> HandlePasswordGrantAsync(
        OpenIddictRequest request,
        UserManager<Account> userManager,
        SignInManager<Account> signInManager,
        IOpenIddictScopeManager scopeManager,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return ForbidTokenRequest();
        }

        Account? user = await userManager.FindByEmailAsync(request.Username)
            ?? await userManager.FindByNameAsync(request.Username);

        if (user is null)
        {
            return ForbidTokenRequest();
        }

        SignInResult signInResult = await signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (!signInResult.Succeeded || !await signInManager.CanSignInAsync(user))
        {
            return ForbidTokenRequest();
        }

        IList<string> roles = await userManager.GetRolesAsync(user);

        ClaimsIdentity identity = ConnectClaims.CreateUserIdentity(user, roles);
        identity.SetScopes(request.GetScopes());

        List<string> resources = await scopeManager
            .ListResourcesAsync(identity.GetScopes(), cancellationToken)
            .ToListAsync(cancellationToken);

        identity.SetResources(resources);
        identity.SetAudiences(resources);
        identity.SetDestinations(ConnectClaims.GetDestinations);

        return Results.SignIn(
            new ClaimsPrincipal(identity),
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static IResult ForbidTokenRequest() =>
        Results.Forbid(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);

    private static async Task<IResult> HandleClientCredentialsAsync(
        OpenIddictRequest request,
        IOpenIddictScopeManager scopeManager,
        CancellationToken cancellationToken)
    {
        var identity = new ClaimsIdentity(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        identity.SetClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? string.Empty);
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.Role, AuthRoles.SERVICE));

        identity.SetScopes(request.GetScopes());

        List<string> resources = await scopeManager
            .ListResourcesAsync(identity.GetScopes(), cancellationToken)
            .ToListAsync(cancellationToken);

        identity.SetResources(resources);
        identity.SetAudiences(resources);

        identity.SetDestinations(static _ => [OpenIddictConstants.Destinations.AccessToken]);

        return Results.SignIn(
            new ClaimsPrincipal(identity),
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IResult> HandleUserGrantAsync(
        HttpContext httpContext,
        OpenIddictRequest request,
        UserManager<Account> userManager,
        IOpenIddictScopeManager scopeManager)
    {
        ClaimsPrincipal? principal = (await httpContext.AuthenticateAsync(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

        string? userId = principal?.GetClaim(OpenIddictConstants.Claims.Subject);
        Account? user = userId is not null
            ? await userManager.FindByIdAsync(userId)
            : null;

        if (user is null || principal is null)
        {
            return Results.Forbid(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }

        if (request.IsRefreshTokenGrantType() && IsSecurityStampStale(principal, user))
        {
            return Results.Forbid(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }

        IList<string> roles = await userManager.GetRolesAsync(user);

        ClaimsIdentity identity = ConnectClaims.CreateUserIdentity(user, roles);
        identity.SetScopes(principal.GetScopes());

        List<string> resources = await scopeManager
            .ListResourcesAsync(identity.GetScopes(), httpContext.RequestAborted)
            .ToListAsync(httpContext.RequestAborted);

        identity.SetResources(resources);
        identity.SetAudiences(resources);

        identity.SetDestinations(ConnectClaims.GetDestinations);

        return Results.SignIn(
            new ClaimsPrincipal(identity),
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static bool IsSecurityStampStale(ClaimsPrincipal principal, Account user)
    {
        string? stampFromToken = principal.GetClaim(AuthClaimTypes.SECURITY_STAMP);

        return !string.IsNullOrEmpty(stampFromToken)
               && !string.Equals(stampFromToken, user.SecurityStamp, StringComparison.Ordinal);
    }
}
