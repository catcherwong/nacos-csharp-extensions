namespace Refit.Extensions.Nacos
{
    using global::Nacos.V2;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using Refit;

    public static class NacosDiscoveryClientExtensions
    {
        public static IHttpClientBuilder AddNacosDiscoveryTypedClient<TInterface>(this IServiceCollection services, string group = "DEFAULT_GROUP", string cluster = "DEFAULT")
            where TInterface : class
        {
            return services.AddNacosDiscoveryTypedClient<TInterface>(p => null, group, cluster);
        }

        public static IHttpClientBuilder AddNacosDiscoveryTypedClient<TInterface>(
            this IServiceCollection services,
            Func<IServiceProvider, RefitSettings> settingsAction,
            string group = "DEFAULT_GROUP", string cluster = "DEFAULT")
           where TInterface : class
        {
            return services
                    .AddRefitClient<TInterface>(settingsAction)
                    .ConfigurePrimaryHttpMessageHandler(provider =>
                    {
                        var svc = provider.GetRequiredService<INacosNamingService>();
                        var loggerFactory = provider.GetService<ILoggerFactory>();

                        if (svc == null)
                        {
                            throw new InvalidOperationException(
                                "Can not find out INacosNamingService, please register at first");
                        }

                        return new NacosExtensions.Common.NacosDiscoveryHttpClientHandler(svc, group, cluster, loggerFactory);
                    });
        }

        // fot test
        internal static IHttpClientBuilder AddNacosDiscoveryTypedClient<TInterface>(
            this IServiceCollection services,
            Func<IServiceProvider, RefitSettings> settingsAction,
            INacosNamingService svc,
            ILoggerFactory loggerFactory,
            string group = "DEFAULT_GROUP",
            string cluster = "DEFAULT")
            where TInterface : class
        {
            return services
                    .AddRefitClient<TInterface>(settingsAction)
                    .ConfigurePrimaryHttpMessageHandler(provider =>
                    {
                        return new NacosExtensions.Common.NacosDiscoveryHttpClientHandler(svc, group, cluster, loggerFactory);
                    });
        }
    }
}
