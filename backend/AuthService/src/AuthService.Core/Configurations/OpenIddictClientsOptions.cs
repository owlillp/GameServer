namespace AuthService.Core.Configurations;

public sealed class OpenIddictClientsOptions
{
    public OpenIddictClientOptions Web { get; init; } = new();

    //public OpenIdDictClientOptions Other { get; init; } = new();
}