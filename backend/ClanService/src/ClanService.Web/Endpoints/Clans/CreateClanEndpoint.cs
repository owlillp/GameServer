using ClanService.Contracts.Clans;
using ClanService.Core.Features.Clans.Commands.CreateClan;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Endpoints;

namespace ClanService.Web.Endpoints.Clans;

public sealed class CreateClanEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/clans", async Task<EndpointResult<ClanSummaryDto>> (
                [FromBody] CreateClanRequest request,
                [FromServices] CreateClanHandler handler,
                CancellationToken ct) =>
            await handler.Handle(new CreateClanCommand(request), ct));
}
