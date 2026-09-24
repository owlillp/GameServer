using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain;

public sealed class Account : IdentityUser<Guid>
{
    public string? DisplayName { get; private set; }

    public UserProfile Profile { get; private set; } = UserProfile.Empty;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    // EF Core
    private Account() { }

    public Account(string email, string userName)
    {
        Id = Guid.CreateVersion7();
        Email = email;
        UserName = userName;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Profile = UserProfile.Empty;
    }

    public void SetDisplayName(string? displayName)
    {
        DisplayName = displayName;
        UpdatedAt = DateTime.UtcNow;
    }
}