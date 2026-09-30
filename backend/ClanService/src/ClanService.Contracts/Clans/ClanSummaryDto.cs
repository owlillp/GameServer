namespace ClanService.Contracts.Clans;

public sealed record ClanSummaryDto(
    Guid Id,
    string Name,
    string Tag,
    Guid LeaderId,
    int MemberCount,
    DateTime CreatedAt);
