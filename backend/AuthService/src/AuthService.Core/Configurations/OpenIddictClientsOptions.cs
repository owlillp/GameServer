namespace AuthService.Core.Configurations;

public sealed class OpenIddictClientsOptions
{
    public OpenIddictClientOptions Web { get; init; } = new();

    // Нативные клиенты (Unity и т.п.): public + PKCE, redirect на loopback
    // или custom scheme.
    public OpenIddictClientOptions Game { get; init; } = new();

    public OpenIddictClientOptions Service { get; init; } = new();
}

public sealed class OpenIddictClientOptions
{
    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public IReadOnlyList<string> RedirectUris { get; init; } = [];

    public IReadOnlyList<string> PostLogoutRedirectUris { get; init; } = [];

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ClientId);

    public bool IsPublic => string.IsNullOrWhiteSpace(ClientSecret);
}
