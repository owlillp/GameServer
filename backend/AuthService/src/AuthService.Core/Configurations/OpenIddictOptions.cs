namespace AuthService.Core.Configurations;

public sealed class OpenIddictOptions
{
    public const string SECTION_NAME = "OpenIddict";

    public string? Issuer { get; init; } = string.Empty;

    public OpenIddictClientsOptions Clients { get; init; } = new();

    public int AccessTokenLifetimeMinutes { get; init; }

    public int RefreshTokenLifetimeDays { get; init; }
}