using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain;

public sealed class Account : IdentityUser<Guid>
{
    public string? DisplayName { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private Account() { }

    public Account(string email, string userName)
    {
        Id = Guid.CreateVersion7();
        Email = email;
        UserName = userName;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDisplayName(string? displayName)
    {
        DisplayName = displayName;
        UpdatedAt = DateTime.UtcNow;
    }
}