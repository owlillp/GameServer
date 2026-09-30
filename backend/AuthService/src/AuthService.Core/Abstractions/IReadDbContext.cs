using AuthService.Domain;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Core.Abstractions;

public interface IReadDbContext
{
    IQueryable<Account> AccountsRead { get; }

    IQueryable<IdentityUserRole<Guid>> AccountRolesRead { get; }

    IQueryable<Role> RolesRead { get; }
}
