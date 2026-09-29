using AuthService.Core.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;

namespace AuthService.Infrastructure.Postgres.Seeding;

public sealed class OidcServerSeeder(
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictScopeManager scopeManager,
    IOptions<OidcSettings> options,
    ILogger<OidcServerSeeder> logger)
{
    private readonly OidcSettings _settings = options.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedScopeAsync(
            OidcScopes.AUTH,
            "AuthService API",
            OidcScopes.AUTH_RESOURCE,
            cancellationToken);

        foreach (OidcClientSettings client in _settings.Clients)
        {
            await SeedClientAsync(client, cancellationToken);
        }

        logger.LogInformation("OpenIdDict scopes and clients seed completed");
    }

    private async Task SeedScopeAsync(
        string name,
        string displayName,
        string resource,
        CancellationToken cancellationToken)
    {
        if (await scopeManager.FindByNameAsync(name, cancellationToken) is not null)
        {
            return;
        }

        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = name,
            DisplayName = displayName,
        };
        descriptor.Resources.Add(resource);

        await scopeManager.CreateAsync(descriptor, cancellationToken);
    }

    private async Task SeedClientAsync(
        OidcClientSettings client,
        CancellationToken cancellationToken)
    {
        bool isConfidential = !string.IsNullOrEmpty(client.ClientSecret);
        bool isServiceClient = client.RedirectUris.Count == 0;

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = client.ClientId,
            DisplayName = client.DisplayName,
            ClientSecret = isConfidential ? client.ClientSecret : null,
            ClientType = isConfidential
                ? OpenIddictConstants.ClientTypes.Confidential
                : OpenIddictConstants.ClientTypes.Public,
            ConsentType = isServiceClient ? null : OpenIddictConstants.ConsentTypes.Implicit,
        };

        foreach (string redirectUri in client.RedirectUris)
        {
            descriptor.RedirectUris.Add(new Uri(redirectUri, UriKind.Absolute));
        }

        foreach (string permission in BuildPermissions(isServiceClient, client.AllowedScopes))
        {
            descriptor.Permissions.Add(permission);
        }

        object? existing = await applicationManager.FindByClientIdAsync(client.ClientId, cancellationToken);
        if (existing is null)
        {
            await applicationManager.CreateAsync(descriptor, cancellationToken);
            return;
        }

        // Идемпотентно добавляем отсутствующие RedirectUris и permissions из конфига.
        // Secret и ClientType существующего клиента не перезаписываем.
        var current = new OpenIddictApplicationDescriptor();
        await applicationManager.PopulateAsync(current, existing, cancellationToken);

        bool changed = false;

        foreach (Uri redirectUri in descriptor.RedirectUris)
        {
            changed |= current.RedirectUris.Add(redirectUri);
        }

        foreach (string permission in descriptor.Permissions)
        {
            changed |= current.Permissions.Add(permission);
        }

        if (!changed)
        {
            return;
        }

        await applicationManager.UpdateAsync(existing, current, cancellationToken);
        logger.LogInformation("OpenID client '{ClientId}' updated", client.ClientId);
    }

    private static IEnumerable<string> BuildPermissions(
        bool isServiceClient,
        IReadOnlyList<string> allowedScopes)
    {
        yield return OpenIddictConstants.Permissions.Endpoints.Token;
        yield return OpenIddictConstants.Permissions.Endpoints.Revocation;

        if (isServiceClient)
        {
            yield return OpenIddictConstants.Permissions.GrantTypes.ClientCredentials;
        }
        else
        {
            yield return OpenIddictConstants.Permissions.Endpoints.Authorization;
            yield return OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode;
            yield return OpenIddictConstants.Permissions.GrantTypes.RefreshToken;
            yield return OpenIddictConstants.Permissions.ResponseTypes.Code;
        }

        foreach (string scope in allowedScopes)
        {
            if (scope is OidcScopes.OPEN_ID or OidcScopes.OFFLINE_ACCESS)
            {
                continue;
            }

            yield return OpenIddictConstants.Permissions.Prefixes.Scope + scope;
        }
    }
}