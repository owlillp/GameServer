namespace AuthService.Contracts.Internal;

/// <summary>Публичные данные аккаунта для других сервисов (internal API).</summary>
public sealed record AuthUserLookupDto(
    Guid UserId,
    string? Name,
    string? UserName,
    string? Email);
