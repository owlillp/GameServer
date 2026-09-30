using ClanService.Core.Features.Clans.Commands.LeaveClan;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Endpoints;

namespace ClanService.Web.Endpoints.Clans;

public sealed class LeaveClanEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/clans/{clanId:guid}/leave", async Task<EndpointResult> (
                [FromRoute] Guid clanId,
                [FromServices] LeaveClanHandler handler,
                CancellationToken ct) =>
            await handler.Handle(new LeaveClanCommand(clanId), ct));
}
