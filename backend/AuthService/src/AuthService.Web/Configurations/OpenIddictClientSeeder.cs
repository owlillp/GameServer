using AuthService.Core.Configurations;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;

namespace AuthService.Web.Configurations;

public sealed class OpenIddictClientSeeder(
    IServiceProvider services,
    IOptions<OpenIddictOptions> options) : IHostedService
{
    private readonly OpenIddictOptions _options = options.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();

        var scopeManager = scope.ServiceProvider
            .GetRequiredService<IOpenIddictScopeManager>();

        var appManager = scope.ServiceProvider
            .GetRequiredService<IOpenIddictApplicationManager>();

        await UpsertOtherScopeAsync(scopeManager, cancellationToken);
        await UpsertWebClientAsync(appManager, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task UpsertOtherScopeAsync(
        IOpenIddictScopeManager scopeManager,
        CancellationToken cancellationToken)
    {
        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = ConnectConstants.Scopes.OTHER_SCOPE,
            DisplayName = "other API Access",
            Resources = { ConnectConstants.Scopes.OTHER_SCOPE },
        };

        object? existing = await scopeManager.FindByNameAsync(descriptor.Name, cancellationToken);
        if (existing is null)
        {
            await scopeManager.CreateAsync(descriptor, cancellationToken);
        }
        else
        {
            await scopeManager.UpdateAsync(descriptor, cancellationToken);
        }
    }

    private async Task UpsertWebClientAsync(
        IOpenIddictApplicationManager applicationManager,
        CancellationToken cancellationToken)
    {
        var client = _options.Clients.Web;
        if (string.IsNullOrWhiteSpace(client.ClientId))
        {
            return;
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = client.ClientId,
            ClientSecret = client.Secret,
            DisplayName = client.DisplayName,
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Roles,
                OpenIddictConstants.Permissions.Prefixes.Scope + ConnectConstants.Scopes.OTHER_SCOPE,
            }
        };

        if (!string.IsNullOrWhiteSpace(client.RedirectUri))
        {
            descriptor.RedirectUris.Add(new Uri(client.RedirectUri));
        }

        await UpsertApplicationAsync(applicationManager, descriptor, cancellationToken);
    }

    private static async Task UpsertApplicationAsync(
        IOpenIddictApplicationManager applicationManager,
        OpenIddictApplicationDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        object? existing = await applicationManager.FindByClientIdAsync(descriptor.ClientId!, cancellationToken);
        if (existing is null)
        {
            await applicationManager.CreateAsync(descriptor, cancellationToken);
        }
        else
        {
            await applicationManager.UpdateAsync(descriptor, cancellationToken);
        }
    }
}