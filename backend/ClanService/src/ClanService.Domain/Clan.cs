namespace ClanService.Domain;

public sealed class Clan
{
    private readonly List<ClanMember> _members = [];

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    // Верхний регистр — для регистронезависимого поиска и уникальности.
    public string NormalizedName { get; private set; } = string.Empty;

    // Тег хранится в верхнем регистре ([A-Z0-9] по правилам валидации).
    public string Tag { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public Guid LeaderId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<ClanMember> Members => _members;

    // EF Core
    private Clan() { }

    public Clan(string name, string tag, string? description, Guid leaderId)
    {
        Id = Guid.CreateVersion7();
        Name = name.Trim();
        NormalizedName = Name.ToUpperInvariant();
        Tag = tag.Trim().ToUpperInvariant();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        LeaderId = leaderId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        _members.Add(new ClanMember(Id, leaderId));
    }

    public bool IsLeader(Guid userId) => LeaderId == userId;

    public bool IsMember(Guid userId) => _members.Any(member => member.UserId == userId);

    public bool TryAddMember(Guid userId)
    {
        if (IsMember(userId))
        {
            return false;
        }

        _members.Add(new ClanMember(Id, userId));
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public bool TryRemoveMember(Guid userId)
    {
        ClanMember? member = _members.FirstOrDefault(member => member.UserId == userId);
        if (member is null)
        {
            return false;
        }

        _members.Remove(member);
        UpdatedAt = DateTime.UtcNow;
        return true;
    }
}
