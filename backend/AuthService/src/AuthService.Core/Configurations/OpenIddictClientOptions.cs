namespace AuthService.Core.Configurations;

public sealed class OpenIddictClientOptions
{
    public string ClientId { get; init; } = string.Empty;

    public string Secret { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string RedirectUri { get; init; } = string.Empty;
}