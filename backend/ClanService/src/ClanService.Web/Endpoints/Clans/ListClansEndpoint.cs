using ClanService.Contracts.Clans;
using ClanService.Core.Features.Clans.Queries.ListClans;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Endpoints;
using Shared.SharedKernel.Responses;

namespace ClanService.Web.Endpoints.Clans;

public sealed class ListClansEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/clans", async Task<EndpointResult<PaginationResponse<ClanSummaryDto>>> (
                [FromQuery] int page,
                [FromQuery] int pageSize,
                [FromQuery] string? search,
                [FromServices] ListClansHandler handler,
                CancellationToken ct) =>
            await handler.Handle(new ListClansQuery(page, pageSize, search), ct));
}
