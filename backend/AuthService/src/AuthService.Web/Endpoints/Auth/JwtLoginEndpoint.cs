using AuthService.Contracts.Requests;
using AuthService.Contracts.Responses;
using AuthService.Core.Features.Auth.Commands.JwtLogin;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Endpoints;

namespace AuthService.Web.Endpoints.Auth;

public sealed class JwtLoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/auth/jwt/login", HandleAsync);

    private static async Task<EndpointResult<JwtLoginResponse>> HandleAsync(
        [FromBody] LoginRequest request,
        [FromServices] JwtLoginHandler handler,
        CancellationToken ct)
        => await handler.Handle(new JwtLoginCommand(request), ct);
}