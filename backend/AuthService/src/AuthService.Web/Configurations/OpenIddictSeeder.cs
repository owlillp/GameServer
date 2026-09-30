using AuthService.Core.Configurations;
using AuthService.Core.Services;
using Microsoft.Extensions.Options;

namespace AuthService.Web.Configurations;

public sealed class OpenIddictSeeder(
    IServiceProvider serviceProvider,
    IOptions<OpenIddictOptions> options,
    ILogger<OpenIddictSeeder> logger) : BackgroundService
{
    private static readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
                PlatformConfigSyncService syncService = scope.ServiceProvider
                    .GetRequiredService<PlatformConfigSyncService>();

                await syncService.SyncAsync(options.Value, stoppingToken);

                logger.LogInformation("OpenIddict configuration synchronized");
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to synchronize OpenIddict configuration, retrying in {Delay}", _retryDelay);
                await Task.Delay(_retryDelay, stoppingToken);
            }
        }
    }
}
