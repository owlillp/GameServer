namespace AuthService.Contracts.Responses;

public sealed record JwtLoginResponse
{
    public string AccessToken { get; init; } = null!;
    public DateTime ExpiresAt { get; init; }
}