using AuthService.Contracts.Responses;

namespace AuthService.Core.Features.Auth.Commands.JwtLogin;

public sealed record JwtLoginResult(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt)
{
    public JwtLoginResponse ToResponse()
        => new () { AccessToken = AccessToken, ExpiresAt = ExpiresAt, };
}