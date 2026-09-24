namespace AuthService.Core.Features.Auth.Services;

public interface IRefreshTokenCookieService
{
    string? GetRefreshToken();

    void Append(string refreshToken, DateTime expiresAt);

    void Delete();
}