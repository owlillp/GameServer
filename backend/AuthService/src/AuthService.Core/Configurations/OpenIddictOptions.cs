namespace AuthService.Core.Configurations;

public sealed class OpenIddictOptions
{
    public const string SECTION_NAME = "OpenIddict";

    public string? Issuer { get; init; }

    public int AccessTokenLifetimeMinutes { get; init; } = 15;

    public int RefreshTokenLifetimeDays { get; init; } = 30;

    public OpenIddictClientsOptions Clients { get; init; } = new();
}
