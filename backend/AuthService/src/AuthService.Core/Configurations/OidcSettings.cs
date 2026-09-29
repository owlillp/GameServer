namespace AuthService.Core.Configurations;

public sealed class OidcSettings
{
    public const string SECTION_NAME = "Oidc";

    public bool Enabled { get; init; } = true;

    public bool DisableTransportSecurityRequirement { get; init; }

    public string Issuer { get; init; } = string.Empty;

    public string FrontendLoginUrl { get; init; } = string.Empty;

    public bool UseDevelopmentCertificates { get; init; }

    public string SigningCertificatePath { get; init; } = string.Empty;

    public string SigningCertificatePassword { get; init; } = string.Empty;

    public string EncryptionCertificatePath { get; init; } = string.Empty;

    public string EncryptionCertificatePassword { get; init; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; init; } = 15;

    public int RefreshTokenLifetimeDays { get; init; } = 30;

    public IReadOnlyList<OidcClientSettings> Clients { get; init; } = [];
}
