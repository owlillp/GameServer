namespace ClanService.Contracts.Clans;

public sealed record CreateClanRequest(
    string Name,
    string Tag,
    string? Description);
