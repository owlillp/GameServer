using ClanService.Core.Authorization;
using ClanService.Core.Features.Clans.Commands.KickMember;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.Authorization;
using Shared.Framework.Endpoints;

namespace ClanService.Web.Endpoints.Clans;

/// <summary>Исключение участника из клана — только для роли Admin.</summary>
public sealed class KickMemberEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/clans/{clanId:guid}/members/{userId:guid}/kick", async Task<EndpointResult> (
                [FromRoute] Guid clanId,
                [FromRoute] Guid userId,
                [FromServices] KickMemberHandler handler,
                CancellationToken ct) =>
            await handler.Handle(new KickMemberCommand(clanId, userId), ct))
            .RequirePermissions(ClanPermissions.CLANS_ADMIN);
}
