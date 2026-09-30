using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain;

public sealed class Role : IdentityRole<Guid>
{
    // EF Core
    private Role() { }

    public Role(string roleName) : base(roleName)
        => Id = Guid.CreateVersion7();
}