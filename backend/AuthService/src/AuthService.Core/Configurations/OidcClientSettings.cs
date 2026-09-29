namespace AuthService.Core.Configurations;

public sealed class OidcClientSettings
{
    public const int MIN_CLIENT_SECRET_LENGTH = 32;

    public string ClientId { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public IReadOnlyList<string> RedirectUris { get; init; } = [];

    public IReadOnlyList<string> AllowedScopes { get; init; } = [];
}
