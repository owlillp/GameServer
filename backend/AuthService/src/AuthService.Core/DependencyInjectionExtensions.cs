using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AuthService.Core.Configurations;
using AuthService.Core.Features.Auth.Services;
using AuthService.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Core.Abstractions;
using Shared.Framework.Authentication;

namespace AuthService.Core;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCore(IConfiguration configuration)
        {
            services.AddHandlers(typeof(DependencyInjectionExtensions).Assembly);
            services.AddValidatorsFromAssembly(typeof(DependencyInjectionExtensions).Assembly);

            services.AddSingleton(TimeProvider.System);
            services.AddHttpContextAccessor();

            services.AddIdentity(configuration);

            return services;
        }

        private IServiceCollection AddIdentity(IConfiguration configuration)
        {
            services.AddCookieAuth(configuration);
            services.AddJwtAuth(configuration);

            return services;
        }

        private void AddCookieAuth(IConfiguration configuration)
        {
            var identitySection = configuration.GetSection(IdentitySettings.SECTION_NAME);
            services.Configure<IdentitySettings>(identitySection);
            var settings = identitySection.Get<IdentitySettings>() ?? new IdentitySettings();

            services.AddIdentity<Account, Role>(options =>
            {
                options.Password.RequiredLength = settings.Password.RequiredLength;
                options.Password.RequireDigit = settings.Password.RequireDigit;
                options.Password.RequireLowercase = settings.Password.RequireLowercase;
                options.Password.RequireUppercase = settings.Password.RequireUppercase;
                options.Password.RequireNonAlphanumeric = settings.Password.RequireNonAlphanumeric;

                options.Lockout.MaxFailedAccessAttempts = settings.Lockout.MaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = settings.Lockout.DefaultLockoutTimeSpan;

                options.User.RequireUniqueEmail = settings.User.RequireUniqueEmail;

                options.SignIn.RequireConfirmedEmail = settings.SignIn.RequireConfirmedEmail;
            }).AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });
        }

        private void AddJwtAuth(IConfiguration configuration)
        {
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IRefreshTokenCookieService, RefreshTokenCookieService>();

            var jwtSection = configuration.GetSection(JwtSettings.SECTION_NAME);
            services.Configure<JwtSettings>(jwtSection);
            var jwt = jwtSection.Get<JwtSettings>() ?? new JwtSettings();

            if (string.IsNullOrWhiteSpace(jwt.SigningKey))
            {
                throw new InvalidOperationException(
                    $"'{JwtSettings.SECTION_NAME}:SigningKey' is not configured. "
                    + "Set Jwt:SigningKey in appsettings or JWT__SIGNINGKEY env var.");
            }

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services
                .AddAuthentication()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                        ClockSkew = TimeSpan.Zero,
                        NameClaimType = AuthClaimTypes.NAME,
                        RoleClaimType = AuthClaimTypes.ROLE,
                    };
                });
        }
    }
}