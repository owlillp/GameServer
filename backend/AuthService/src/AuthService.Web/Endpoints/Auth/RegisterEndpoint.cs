using AuthService.Contracts.Requests;
using AuthService.Contracts.Responses;
using AuthService.Core.Features.Auth.Commands.Register;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Endpoints;

namespace AuthService.Web.Endpoints.Auth;

public sealed class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/auth/register", async Task<EndpointResult<RegisterResponse>> (
                [FromBody] RegisterRequest request,
                [FromServices] RegisterHandler handler,
                CancellationToken ct) =>
            await handler.Handle(new RegisterCommand(request), ct));
}