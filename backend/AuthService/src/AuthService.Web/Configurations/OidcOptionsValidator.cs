using AuthService.Core.Configurations;
using Microsoft.Extensions.Options;

namespace AuthService.Web.Configurations;

public sealed class OidcOptionsValidator(IHostEnvironment environment) : IValidateOptions<OidcSettings>
{
    public ValidateOptionsResult Validate(string? name, OidcSettings options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (!Uri.TryCreate(options.Issuer, UriKind.Absolute, out Uri? issuer) ||
            !string.Equals(issuer.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return ValidateOptionsResult.Fail("Oidc:Issuer must be an absolute HTTPS URI");
        }

        if (!string.IsNullOrEmpty(options.FrontendLoginUrl)
            && !Uri.TryCreate(options.FrontendLoginUrl, UriKind.Absolute, out _))
        {
            return ValidateOptionsResult.Fail("Oidc:FrontendLoginUrl must be an absolute URI when set");
        }

        if (options.AccessTokenLifetimeMinutes <= 0)
        {
            return ValidateOptionsResult.Fail("Oidc:AccessTokenLifetimeMinutes must be positive");
        }

        if (options.RefreshTokenLifetimeDays <= 0)
        {
            return ValidateOptionsResult.Fail("Oidc:RefreshTokenLifetimeDays must be positive");
        }

        var clientIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (OidcClientSettings client in options.Clients)
        {
            if (string.IsNullOrWhiteSpace(client.ClientId))
            {
                return ValidateOptionsResult.Fail("Each Oidc client must have ClientId");
            }

            if (!clientIds.Add(client.ClientId))
            {
                return ValidateOptionsResult.Fail($"Oidc client '{client.ClientId}' is duplicated");
            }

            if (string.IsNullOrWhiteSpace(client.DisplayName))
            {
                return ValidateOptionsResult.Fail($"Oidc client '{client.ClientId}' must have DisplayName");
            }

            if (!string.IsNullOrEmpty(client.ClientSecret)
                && client.ClientSecret.Length < OidcClientSettings.MIN_CLIENT_SECRET_LENGTH)
            {
                return ValidateOptionsResult.Fail(
                    $"Oidc client '{client.ClientId}' secret must have at least "
                    + $"{OidcClientSettings.MIN_CLIENT_SECRET_LENGTH} characters");
            }

            if (client.RedirectUris.Count == 0 && string.IsNullOrEmpty(client.ClientSecret))
            {
                return ValidateOptionsResult.Fail($"Oidc client '{client.ClientId}' must define RedirectUris or a ClientSecret");
            }
            else if (client.RedirectUris.Any(uri => !Uri.TryCreate(uri, UriKind.Absolute, out _)))
            {
                return ValidateOptionsResult.Fail(
                    $"Oidc client '{client.ClientId}' must have valid absolute RedirectUris");
            }

            if (client.AllowedScopes.Count == 0
                || client.AllowedScopes.Any(scope => !OidcScopes.All.Contains(scope)))
            {
                return ValidateOptionsResult.Fail($"Oidc client '{client.ClientId}' contains missing or unsupported AllowedScopes");
            }
        }

        if (environment.IsEnvironment("Testing"))
        {
            return ValidateOptionsResult.Success;
        }

        bool allowsDevelopmentCertificates = environment.IsDevelopment() || environment.IsEnvironment("Docker");

        if (options.DisableTransportSecurityRequirement
            && !allowsDevelopmentCertificates
            && !environment.IsEnvironment("Testing"))
        {
            return ValidateOptionsResult.Fail("Oidc:DisableTransportSecurityRequirement is allowed only in Development, Docker or Testing");
        }

        if (options.UseDevelopmentCertificates)
        {
            return allowsDevelopmentCertificates
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail("Oidc:UseDevelopmentCertificates is allowed only in Development or Docker");
        }

        if (string.IsNullOrWhiteSpace(options.SigningCertificatePath))
        {
            return ValidateOptionsResult.Fail("Oidc:SigningCertificatePath is required");
        }

        if (string.IsNullOrWhiteSpace(options.EncryptionCertificatePath))
        {
            return ValidateOptionsResult.Fail("Oidc:EncryptionCertificatePath is required");
        }

        return ValidateOptionsResult.Success;
    }
}
