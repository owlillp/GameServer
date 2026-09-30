namespace ClanService.Domain;

public sealed class ClanMember
{
    public Guid ClanId { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime JoinedAt { get; private set; }

    // EF Core
    private ClanMember() { }

    internal ClanMember(Guid clanId, Guid userId)
    {
        ClanId = clanId;
        UserId = userId;
        JoinedAt = DateTime.UtcNow;
    }
}
