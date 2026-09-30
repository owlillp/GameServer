using System.Security.Cryptography;
using System.Text;
using AuthService.Core.Configurations;
using AuthService.Core.Features.Connect;
using AuthService.Infrastructure.Postgres;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;

namespace AuthService.Web.Configurations;

public static class OpenIddictConfigurationExtensions
{
    public static IServiceCollection AddAuthServiceOpenIddict(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddOptions<OpenIddictOptions>()
            .Bind(configuration.GetSection(OpenIddictOptions.SECTION_NAME))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<OpenIddictOptions>, OpenIddictOptionsValidator>();

        services.Configure<SigningKeyOptions>(configuration.GetSection(SigningKeyOptions.SECTION_NAME));
        services.AddOptions<AuthServiceOptions>().Bind(configuration.GetSection(AuthServiceOptions.SECTION_NAME));

        OpenIddictOptions openIddictOptions = configuration
            .GetSection(OpenIddictOptions.SECTION_NAME)
            .Get<OpenIddictOptions>() ?? new OpenIddictOptions();

        SigningKeyOptions signingKeyOptions = configuration
            .GetSection(SigningKeyOptions.SECTION_NAME)
            .Get<SigningKeyOptions>() ?? new SigningKeyOptions();

        services.AddOpenIddict()
            .AddCore(options => options
                .UseEntityFrameworkCore()
                .UseDbContext<AuthServiceDbContext>()
            ).AddServer(options =>
            {
                if (!string.IsNullOrWhiteSpace(openIddictOptions.Issuer))
                {
                    options.SetIssuer(new Uri(openIddictOptions.Issuer));
                }

                options.AllowAuthorizationCodeFlow();
                options.AllowRefreshTokenFlow();
                options.AllowClientCredentialsFlow();
                options.AllowPasswordFlow();

                options.RequireProofKeyForCodeExchange();
                options.DisableAccessTokenEncryption();

                options.SetAuthorizationEndpointUris(ConnectConstants.AUTHORIZE_ENDPOINT);
                options.SetTokenEndpointUris(ConnectConstants.TOKEN_ENDPOINT);
                options.SetRevocationEndpointUris(ConnectConstants.REVOKE_ENDPOINT);
                options.SetUserInfoEndpointUris(ConnectConstants.USER_INFO_ENDPOINT);
                options.SetEndSessionEndpointUris(ConnectConstants.END_SESSION_ENDPOINT);

                options.RegisterScopes([.. OidcScopes.All]);

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(openIddictOptions.AccessTokenLifetimeMinutes));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(openIddictOptions.RefreshTokenLifetimeDays));
                options.SetRefreshTokenReuseLeeway(TimeSpan.Zero);

                AddSigningKeys(options, environment, signingKeyOptions);

                var aspNetCore = options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough();

                if (IsLocalInsecureEnvironment(environment))
                {
                    aspNetCore.DisableTransportSecurityRequirement();
                }
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        services.AddHostedService<OpenIddictSeeder>();

        return services;
    }

    private static bool IsLocalInsecureEnvironment(IHostEnvironment environment)
        => environment.IsDevelopment() || environment.IsEnvironment("Docker");

    private static void AddSigningKeys(
        OpenIddictServerBuilder builder,
        IWebHostEnvironment environment,
        SigningKeyOptions keys)
    {
        if (keys.IsConfigured)
        {
            builder.AddSigningKey(ImportRsaKey(keys.SigningKeyBase64!));
            builder.AddEncryptionKey(ImportRsaKey(keys.EncryptionKeyBase64!));
            return;
        }

        if (environment.IsProduction())
        {
            throw new InvalidOperationException(
                "Production signing/encryption keys are required. "
                + "Set SigningKeys:SigningKeyBase64 and SigningKeys:EncryptionKeyBase64.");
        }

        builder.AddDevelopmentSigningCertificate();
        builder.AddDevelopmentEncryptionCertificate();
    }

    private static RsaSecurityKey ImportRsaKey(string base64Pem)
    {
        byte[] pem = Convert.FromBase64String(base64Pem);
        RSA rsa = RSA.Create();
        rsa.ImportFromPem(Encoding.UTF8.GetString(pem));
        return new RsaSecurityKey(rsa);
    }
}
