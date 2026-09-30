using System.Security.Cryptography;
using System.Text;
using AuthService.Core.Configurations;
using AuthService.Infrastructure.Postgres;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace AuthService.Web.Configurations;

public static class OidcServerConfigurationExtensions
{
    public static IServiceCollection AddAuthServiceOidcServer(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddOptions<OpenIddictOptions>()
            .Bind(configuration.GetSection(OpenIddictOptions.SECTION_NAME))
            .ValidateOnStart();

        var openIdDictOptions = configuration
            .GetSection(OpenIddictOptions.SECTION_NAME)
            .Get<OpenIddictOptions>() ?? new OpenIddictOptions();

        services.AddOpenIddict()
            .AddCore(options => options
                .UseEntityFrameworkCore()
                .UseDbContext<AuthServiceDbContext>()
                .ReplaceDefaultEntities<Guid>()
            ).AddServer(options =>
            {
                if (!string.IsNullOrWhiteSpace(openIdDictOptions.Issuer))
                {
                    options.SetIssuer(new Uri(openIdDictOptions.Issuer));
                }

                options.AllowAuthorizationCodeFlow();
                options.AllowRefreshTokenFlow();
                options.AllowClientCredentialsFlow();

                options.RequireProofKeyForCodeExchange();
                options.DisableAccessTokenEncryption();

                options.SetAuthorizationEndpointUris(ConnectConstants.AUTHORIZE_ENDPOINT);
                options.SetTokenEndpointUris(ConnectConstants.TOKEN_ENDPOINT);
                options.SetRevocationEndpointUris(ConnectConstants.REVOKE_ENDPOINT);
                options.SetUserInfoEndpointUris(ConnectConstants.USER_INFO_ENDPOINT);

                options.RegisterScopes(
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.Roles,
                    ConnectConstants.Scopes.OTHER_SCOPE);

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(openIdDictOptions.AccessTokenLifetimeMinutes));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(openIdDictOptions.RefreshTokenLifetimeDays));

                options.SetRefreshTokenReuseLeeway(TimeSpan.Zero);

                AddSigningKeys(options, environment, signingKeys);

                var aspNetCore = options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough();

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

        services.AddHostedService<OpenIddictClientSeeder>();

        return services;
    }

    private static bool IsLocalInsecureEnvironment(IHostEnvironment environment)
        => environment.IsDevelopment() || environment.IsEnvironment("Docker");

    private static void AddSigningKeys(
        OpenIddictServerBuilder builder,
        IWebHostEnvironment environment,
        SigningKeysOptions keys)
    {
        string? signingPem = keys.SigningKeyBase64;
        string? encryptionPem = keys.EncryptionKeyBase64;

        if (!string.IsNullOrWhiteSpace(signingPem) && !string.IsNullOrWhiteSpace(encryptionPem))
        {
            builder.AddSigningKey(ImportRsaKey(signingPem));
            builder.AddEncryptionKey(ImportRsaKey(encryptionPem));
        }
        else if(environment.IsProduction())
        {
            throw new InvalidOperationException("Production signing/encryption keys are required");
        }
        else
        {
            builder.AddDevelopmentSigningCertificate();
            builder.AddDevelopmentEncryptionCertificate();
        }
    }

    private static RsaSecurityKey ImportRsaKey(string base64Pem)
    {
        byte[] pem = Convert.FromBase64String(base64Pem);
        var rsa = RSA.Create();
        rsa.ImportFromPem(Encoding.UTF8.GetString(pem));
        return  new RsaSecurityKey(rsa);
    }
}
