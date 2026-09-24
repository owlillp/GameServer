namespace AuthService.Contracts.Responses;

public record JwtRefreshResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt)
{
    public JwtLoginResponse ToResponse()
        => new () { AccessToken = AccessToken, ExpiresAt = ExpiresAt, };
}