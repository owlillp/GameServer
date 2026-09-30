using Microsoft.Extensions.Options;

namespace AuthService.Core.Configurations;

public sealed class OpenIddictOptionsValidator : IValidateOptions<OpenIddictOptions>
{
    public ValidateOptionsResult Validate(string? name, OpenIddictOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (!string.IsNullOrWhiteSpace(options.Issuer)
            && !Uri.TryCreate(options.Issuer, UriKind.Absolute, out _))
        {
            failures.Add($"{OpenIddictOptions.SECTION_NAME}:Issuer must be an absolute URI.");
        }

        if (options.AccessTokenLifetimeMinutes <= 0)
        {
            failures.Add($"{OpenIddictOptions.SECTION_NAME}:AccessTokenLifetimeMinutes must be positive.");
        }

        if (options.RefreshTokenLifetimeDays <= 0)
        {
            failures.Add($"{OpenIddictOptions.SECTION_NAME}:RefreshTokenLifetimeDays must be positive.");
        }

        ValidateClient(options.Clients.Web, $"{OpenIddictOptions.SECTION_NAME}:Clients:Web", failures, requireSecret: false);
        ValidateClient(options.Clients.Game, $"{OpenIddictOptions.SECTION_NAME}:Clients:Game", failures, requireSecret: false);
        ValidateClient(options.Clients.Service, $"{OpenIddictOptions.SECTION_NAME}:Clients:Service", failures, requireSecret: true);

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    private static void ValidateClient(
        OpenIddictClientOptions client,
        string section,
        List<string> failures,
        bool requireSecret)
    {
        if (!client.IsConfigured)
        {
            return;
        }

        if (requireSecret && client.IsPublic)
        {
            failures.Add($"{section}:ClientSecret is required for a confidential client.");
        }

        foreach (string redirectUri in client.RedirectUris)
        {
            if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out _))
            {
                failures.Add($"{section}:RedirectUris contains an invalid URI: '{redirectUri}'.");
            }
        }

        foreach (string redirectUri in client.PostLogoutRedirectUris)
        {
            if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out _))
            {
                failures.Add($"{section}:PostLogoutRedirectUris contains an invalid URI: '{redirectUri}'.");
            }
        }
    }
}
