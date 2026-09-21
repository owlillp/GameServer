using AuthService.Contracts.Requests;
using AuthService.Contracts.Responses;
using AuthService.Core.Features.Auth.Commands.Login;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Endpoints;

namespace AuthService.Web.Endpoints.Auth;

public sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/auth/login", async Task<EndpointResult<LoginResponse>> (
                [FromBody] LoginRequest request,
                [FromServices] LoginHandler handler,
                CancellationToken ct) =>
            await handler.Handle(new LoginCommand(request), ct));
}