using AuthService.Domain;

namespace AuthService.Core.Features.Auth.Services;

public sealed record JwtToken(string AccessToken, DateTime ExpiresAt);

public interface IJwtTokenService
{
    JwtToken Generate(Account account, IReadOnlyCollection<string> roles);
}