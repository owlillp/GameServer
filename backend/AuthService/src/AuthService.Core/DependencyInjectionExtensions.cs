using AuthService.Core.Configurations;
using AuthService.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

                options.ClaimsIdentity.UserIdClaimType = AuthClaimTypes.SUB;
                options.ClaimsIdentity.UserNameClaimType = AuthClaimTypes.NAME;
                options.ClaimsIdentity.EmailClaimType = AuthClaimTypes.EMAIL;
                options.ClaimsIdentity.RoleClaimType = AuthClaimTypes.ROLE;
                options.ClaimsIdentity.SecurityStampClaimType = AuthClaimTypes.SECURITY_STAMP;
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
    }
}