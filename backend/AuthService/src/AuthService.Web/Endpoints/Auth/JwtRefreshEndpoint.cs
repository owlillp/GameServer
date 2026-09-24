using AuthService.Contracts.Responses;
using AuthService.Core.Features.Auth.Commands.JwtRefresh;
using AuthService.Core.Features.Auth.Services;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Endpoints;

namespace AuthService.Web.Endpoints.Auth;

public sealed class JwtRefreshEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/auth/jwt/refresh", HandleAsync);

    private static async Task<EndpointResult<JwtLoginResponse>> HandleAsync(
        [FromServices] JwtRefreshHandler handler,
        [FromServices] IRefreshTokenCookieService refreshTokenCookie,
        CancellationToken ct)
    {
        var result = await handler.Handle(new JwtRefreshCommand(refreshTokenCookie.GetRefreshToken()), ct);

        if (result.IsSuccess)
        {
            refreshTokenCookie.Append(
                result.Value.RefreshToken,
                result.Value.RefreshTokenExpiresAt);
        }
        else
        {
            refreshTokenCookie.Delete();
        }

        return result.Map(static refresh => refresh.ToResponse());
    }
}