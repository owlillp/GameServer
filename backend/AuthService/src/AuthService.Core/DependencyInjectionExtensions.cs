using AuthService.Core.Configurations;
using AuthService.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Abstractions;

namespace AuthService.Core;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCore(IConfiguration configuration)
        {
            services.AddHandlers(typeof(DependencyInjectionExtensions).Assembly);
            services.AddValidatorsFromAssembly(typeof(DependencyInjectionExtensions).Assembly);

            services.AddIdentity(configuration);

            return services;
        }

        private IServiceCollection AddIdentity(IConfiguration configuration)
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

            return services;
        }
    }
}