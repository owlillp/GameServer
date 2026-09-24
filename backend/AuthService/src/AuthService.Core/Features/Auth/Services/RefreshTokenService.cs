using System.Security.Cryptography;
using System.Text;

namespace AuthService.Core.Features.Auth.Services;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private const int REFRESH_TOKEN_BYTES = 64;

    public string GenerateToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(REFRESH_TOKEN_BYTES);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal);
    }

    public string Hash(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        byte[] bytes = Encoding.UTF8.GetBytes(refreshToken);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}