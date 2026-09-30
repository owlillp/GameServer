namespace ClanService.Contracts.Clans;

public sealed record ClanMemberDto(
    Guid UserId,
    string? Name,
    string? UserName,
    string? Email,
    DateTime JoinedAt);

public sealed record ClanDetailsDto(
    Guid Id,
    string Name,
    string Tag,
    string? Description,
    Guid LeaderId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<ClanMemberDto> Members);
