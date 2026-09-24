namespace AuthService.Domain;

public sealed class RefreshSession
{
    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public Account Account { get; private set; } = null!;

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public Guid? ReplacedById { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;

    // EF Core
    private RefreshSession() { }

    private RefreshSession(Guid accountId, string tokenHash, DateTime createdAt, DateTime expiresAt)
    {
        Id = Guid.CreateVersion7();
        AccountId = accountId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public static RefreshSession Create(
        Guid accountId,
        string tokenHash,
        DateTime createdAt,
        DateTime expiresAt)
        => new(accountId, tokenHash, createdAt, expiresAt);

    public bool IsExpired(DateTime now)
        => ExpiresAt <= now;

    public void RotateTo(RefreshSession replacement, DateTime revokedAt)
    {
        ArgumentNullException.ThrowIfNull(replacement);

        RevokedAt = revokedAt;
        ReplacedById = replacement.Id;
    }

    public void Revoke(DateTime revokedAt)
        => RevokedAt ??= revokedAt;
}