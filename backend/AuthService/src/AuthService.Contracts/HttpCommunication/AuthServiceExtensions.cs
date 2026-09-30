using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Framework.Authentication.HttpClients;

namespace AuthService.Contracts.HttpCommunication;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddAuthServiceHttpCommunication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthServiceOptions>(configuration.GetSection(AuthServiceOptions.SECTION_NAME));

        // ServiceTokenOptions + ServiceTokenProvider из Shared.Framework.
        services.AddServiceTokenForwarding(configuration);
        services.AddTransient<ServiceTokenHandler>();

        services.AddHttpClient<IAuthServiceClient, AuthServiceClient>((sp, client) =>
            {
                AuthServiceOptions options = sp.GetRequiredService<IOptions<AuthServiceOptions>>().Value;

                client.BaseAddress = new Uri(options.Url);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddHttpMessageHandler<ServiceTokenHandler>();

        return services;
    }
}
