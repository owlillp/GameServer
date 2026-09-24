namespace AuthService.Core.Features.Auth.Services;

public interface IRefreshTokenService
{
    string GenerateToken();

    string Hash(string refreshToken);
}