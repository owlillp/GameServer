namespace AuthService.Core.Configurations;

public sealed class JwtSettings
{
    public const string SECTION_NAME = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; set; } = 3;

    public int RefreshTokenLifetimeDays { get; set; } = 14;
}