using Shared.Core.Abstractions;

namespace ClanService.Core.Features.Clans.Queries.ListClans;

public sealed record ListClansQuery(int Page, int PageSize, string? Search) : IQuery;
