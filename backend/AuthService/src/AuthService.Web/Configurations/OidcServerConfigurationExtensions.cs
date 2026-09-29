using System.Security.Cryptography.X509Certificates;
using AuthService.Core.Configurations;
using AuthService.Infrastructure.Postgres;
using AuthService.Infrastructure.Postgres.Seeding;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace AuthService.Web.Configurations;

public static class OidcServerConfigurationExtensions
{
    public static IServiceCollection AddAuthServiceOidcServer(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton<IValidateOptions<OidcSettings>, OidcOptionsValidator>();
        services.AddOptions<OidcSettings>()
            .Bind(configuration.GetSection(OidcSettings.SECTION_NAME))
            .ValidateOnStart();

        var settings = configuration
            .GetSection(OidcSettings.SECTION_NAME)
            .Get<OidcSettings>() ?? new OidcSettings();

        if (!settings.Enabled)
        {
            return services;
        }

        services.AddOpenIddict()
            .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<AuthServiceDbContext>())
            .AddServer(options =>
            {
                options.SetIssuer(new Uri(settings.Issuer, UriKind.Absolute));
                options.SetAuthorizationEndpointUris("/connect/authorize");
                options.SetTokenEndpointUris("/connect/token");
                options.SetRevocationEndpointUris("/connect/revoke");
                options.SetUserInfoEndpointUris("/connect/userinfo");

                options.AllowAuthorizationCodeFlow();
                options.AllowRefreshTokenFlow();
                options.AllowClientCredentialsFlow();
                options.RequireProofKeyForCodeExchange();

                options.RegisterScopes(
                    OidcScopes.OPEN_ID,
                    OidcScopes.PROFILE,
                    OidcScopes.EMAIL,
                    OidcScopes.OFFLINE_ACCESS,
                    OidcScopes.AUTH);

                options.RegisterClaims(
                    OpenIddictConstants.Claims.Name,
                    OpenIddictConstants.Claims.PreferredUsername,
                    OpenIddictConstants.Claims.Email,
                    OpenIddictConstants.Claims.EmailVerified,
                    OpenIddictConstants.Claims.Role);

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(settings.AccessTokenLifetimeMinutes));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(settings.RefreshTokenLifetimeDays));

                options.SetRefreshTokenReuseLeeway(TimeSpan.Zero);
                options.DisableAccessTokenEncryption();

                ConfigureCertificates(options, settings, environment);

                var aspNetCore = options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough();

                if (settings.DisableTransportSecurityRequirement)
                {
                    aspNetCore.DisableTransportSecurityRequirement();
                }
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.AddAudiences(OidcScopes.AUTH_RESOURCE);
                options.UseAspNetCore();
            });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        services.AddScoped<OidcServerSeeder>();

        return services;
    }

    private static void ConfigureCertificates(
        OpenIddictServerBuilder options,
        OidcSettings settings,
        IHostEnvironment environment)
    {
        if (environment.IsEnvironment("Testing"))
        {
            options.AddEphemeralEncryptionKey();
            options.AddEphemeralSigningKey();
            return;
        }

        if (settings.UseDevelopmentCertificates)
        {
            options.AddDevelopmentEncryptionCertificate();
            options.AddDevelopmentSigningCertificate();
            return;
        }

        var signingCertificate = X509CertificateLoader.LoadPkcs12FromFile(
            settings.SigningCertificatePath,
            settings.SigningCertificatePassword);

        var encryptionCertificate = X509CertificateLoader.LoadPkcs12FromFile(
            settings.EncryptionCertificatePath,
            settings.EncryptionCertificatePassword);

        options.AddSigningCertificate(signingCertificate);
        options.AddEncryptionCertificate(encryptionCertificate);
    }
}
