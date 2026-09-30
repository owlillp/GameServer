using ClanService.Contracts.Clans;
using ClanService.Core.Abstractions;
using ClanService.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Abstractions;
using Shared.SharedKernel.Errors;
using Shared.SharedKernel.Responses;

namespace ClanService.Core.Features.Clans.Queries.ListClans;

public sealed class ListClansHandler(IReadDbContext readDbContext)
    : IQueryHandlerWithResult<PaginationResponse<ClanSummaryDto>, ListClansQuery>
{
    private const int DEFAULT_PAGE_SIZE = 20;
    private const int MAX_PAGE_SIZE = 100;

    public async Task<Result<PaginationResponse<ClanSummaryDto>, Error>> Handle(
        ListClansQuery query,
        CancellationToken cancellationToken = new ())
    {
        int page = Math.Max(1, query.Page);
        int pageSize = query.PageSize <= 0
            ? DEFAULT_PAGE_SIZE
            : Math.Min(query.PageSize, MAX_PAGE_SIZE);

        IQueryable<Clan> clansQuery = readDbContext.ClansRead;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim().ToUpperInvariant();

            clansQuery = clansQuery.Where(clan =>
                clan.NormalizedName.Contains(term)
                || clan.Tag.Contains(term));
        }

        int totalCount = await clansQuery.CountAsync(cancellationToken);

        List<ClanSummaryDto> records = await clansQuery
            .OrderByDescending(clan => clan.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(clan => new ClanSummaryDto(
                clan.Id,
                clan.Name,
                clan.Tag,
                clan.LeaderId,
                clan.Members.Count,
                clan.CreatedAt))
            .ToListAsync(cancellationToken);

        int totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));

        return new PaginationResponse<ClanSummaryDto>(records, totalCount, page, pageSize, totalPages);
    }
}
