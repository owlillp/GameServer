namespace AuthService.Contracts.Responses;

public sealed record JwtSessionStatusResponse(
    bool HasRefreshCookie,
    bool IsRefreshSessionActive,
    DateTime? RefreshSessionExpiresAt,
    DateTime? RefreshSessionRevokedAt);