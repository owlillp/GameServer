using ClanService.Core.Features.Clans.Commands.DeleteClan;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Endpoints;

namespace ClanService.Web.Endpoints.Clans;

public sealed class DeleteClanEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapDelete("/clans/{clanId:guid}", async Task<EndpointResult> (
                [FromRoute] Guid clanId,
                [FromServices] DeleteClanHandler handler,
                CancellationToken ct) =>
            await handler.Handle(new DeleteClanCommand(clanId), ct));
}
