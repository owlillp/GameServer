using AuthService.Contracts.Requests;
using AuthService.Contracts.Responses;
using AuthService.Core.Features.Auth.Commands.JwtRegister;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Endpoints;

namespace AuthService.Web.Endpoints.Auth;

public sealed class JwtRegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/auth/jwt/register", async Task<EndpointResult<RegisterResponse>> (
                [FromBody] RegisterRequest request,
                [FromServices] JwtRegisterHandler handler,
                CancellationToken ct) =>
            await handler.Handle(new JwtRegisterCommand(request), ct));
}