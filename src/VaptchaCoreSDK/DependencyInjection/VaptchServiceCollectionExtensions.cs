using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;


namespace VaptchaCoreSDK
{
    public static class VaptchServiceCollectionExtensions
    {
        public static IServiceCollection AddVaptch(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddHttpClient<VaptchaDownCheckHttpClient>();
            services.AddHttpClient<VaptchaHttpClient>();
            services.AddTransient<IVaptchaService, VaptchaService>();
            return services;
        }

        public static IServiceCollection AddVaptch(this IServiceCollection services, Action<VaptchaKeyOptions> configureOptions)
        {
            return services.AddVaptch().ConfigureVaptch(configureOptions);
        }

        public static IServiceCollection AddVaptch(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            return services.AddVaptch().ConfigureVaptch(configurationSection);
        }

        public static IServiceCollection ConfigureVaptch(this IServiceCollection services, Action<VaptchaKeyOptions> configureOptions)
        {
            services.Configure(configureOptions);
            return services;
        }

        public static IServiceCollection ConfigureVaptch(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.Configure<VaptchaKeyOptions>(configurationSection);
            return services;
        }
    }
}
