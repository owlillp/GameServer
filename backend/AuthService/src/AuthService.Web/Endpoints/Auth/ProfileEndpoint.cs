using AuthService.Contracts.Dtos;
using AuthService.Core.Features.Auth.Queries.GetMyProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Endpoints;

namespace AuthService.Web.Endpoints.Auth;

public sealed class ProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/auth/profile",
            [Authorize(AuthenticationSchemes = "Identity.Application,Bearer")]
            async Task<EndpointResult<ProfileDto>> (
                    [FromServices] GetMyProfileHandler handler,
                    CancellationToken ct) =>
                await handler.Handle(new GetMyProfileQuery(), ct));
}