using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Abstractions;

namespace ClanService.Core;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCore()
        {
            services.AddHandlers(typeof(DependencyInjectionExtensions).Assembly);
            services.AddValidatorsFromAssembly(typeof(DependencyInjectionExtensions).Assembly);

            return services;
        }
    }
}
