using AuthService.Domain;

namespace AuthService.Core.Abstractions;

public interface IReadDbContext
{
    IQueryable<Account> AccountsRead { get; }
}