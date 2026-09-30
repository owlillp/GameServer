using Shared.Core.Abstractions;

namespace ClanService.Core.Features.Clans.Queries.GetClan;

public sealed record GetClanQuery(Guid ClanId) : IQuery;
