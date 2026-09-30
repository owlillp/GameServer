using ClanService.Core.Features.Clans.Commands.JoinClan;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Endpoints;

namespace ClanService.Web.Endpoints.Clans;

public sealed class JoinClanEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/clans/{clanId:guid}/join", async Task<EndpointResult> (
                [FromRoute] Guid clanId,
                [FromServices] JoinClanHandler handler,
                CancellationToken ct) =>
            await handler.Handle(new JoinClanCommand(clanId), ct));
}
