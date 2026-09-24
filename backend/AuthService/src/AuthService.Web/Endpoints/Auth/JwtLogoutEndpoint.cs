using AuthService.Core.Features.Auth.Commands.JwtLogout;
using AuthService.Core.Features.Auth.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Endpoints;

namespace AuthService.Web.Endpoints.Auth;

public sealed class JwtLogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/auth/jwt/logout", HandleAsync);

    private static async Task<EndpointResult> HandleAsync(
        [FromServices] JwtLogoutHandler handler,
        [FromServices] IRefreshTokenCookieService refreshTokenCookie,
        CancellationToken ct)
    {
        var result = await handler.Handle(new JwtLogoutCommand(refreshTokenCookie.GetRefreshToken()), ct);
        refreshTokenCookie.Delete();
        return result;
    }
}