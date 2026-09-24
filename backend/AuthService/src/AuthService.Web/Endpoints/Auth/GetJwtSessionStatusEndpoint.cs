using AuthService.Contracts.Responses;
using AuthService.Core.Features.Auth.Queries.GetJwtSesionStatus;
using AuthService.Core.Features.Auth.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Endpoints;

namespace AuthService.Web.Endpoints.Auth;

public sealed class JwtSessionStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/auth/jwt/session", HandleAsync);

    private static async Task<EndpointResult<JwtSessionStatusResponse>> HandleAsync(
        [FromServices] JwtSessionStatusHandler handler,
        [FromServices] IRefreshTokenCookieService refreshTokenCookie,
        CancellationToken ct)
        => await handler.Handle(new JwtSessionStatusQuery(refreshTokenCookie.GetRefreshToken()), ct);
}