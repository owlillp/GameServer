using AuthService.Contracts.Admin;
using AuthService.Core.Features.Admin.Queries.GetStats;
using AuthService.Domain;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Authorization;
using Shared.Framework.Endpoints;

namespace AuthService.Web.Endpoints.Admin;

public sealed class AdminStatsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/auth/admin/stats",
                async Task<EndpointResult<AdminStatsDto>> (
                        [FromServices] GetStatsHandler handler,
                        CancellationToken ct) =>
                    await handler.Handle(new GetStatsQuery(), ct))
            .RequirePermissions(AuthPermissions.Users.VIEW);
}
