using ClanService.Contracts.Clans;
using ClanService.Core.Features.Clans.Queries.GetClan;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Endpoints;

namespace ClanService.Web.Endpoints.Clans;

public sealed class GetClanEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/clans/{clanId:guid}", async Task<EndpointResult<ClanDetailsDto>> (
                [FromRoute] Guid clanId,
                [FromServices] GetClanHandler handler,
                CancellationToken ct) =>
            await handler.Handle(new GetClanQuery(clanId), ct));
}
