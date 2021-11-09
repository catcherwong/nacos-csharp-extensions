namespace WebApiClientCore.Extensions.Nacos
{
    using global::Nacos.V2;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using WebApiClientCore;

    public static class NacosDiscoveryClientExtensions
    {
        public static IHttpClientBuilder AddNacosDiscoveryTypedClient<TInterface>(this IServiceCollection services, string group = "DEFAULT_GROUP", string cluster = "DEFAULT")
           where TInterface : class, IHttpApi
        {
            return services.AddNacosDiscoveryTypedClient<TInterface>(c => { }, group, cluster);
        }

        public static IHttpClientBuilder AddNacosDiscoveryTypedClient<TInterface>(this IServiceCollection services, Action<HttpApiOptions> configOptions, string group = "DEFAULT_GROUP", string cluster = "DEFAULT")
            where TInterface : class, IHttpApi
        {
            if (configOptions == null)
            {
                throw new ArgumentNullException(nameof(configOptions));
            }

            return services.AddNacosDiscoveryTypedClient<TInterface>((c, p) => configOptions.Invoke(c), group, cluster);
        }


        public static IHttpClientBuilder AddNacosDiscoveryTypedClient<TInterface>(this IServiceCollection services, Action<HttpApiOptions, IServiceProvider> configOptions, string group = "DEFAULT_GROUP", string cluster = "DEFAULT")
            where TInterface : class, IHttpApi
        {
            if (configOptions == null)
            {
                throw new ArgumentNullException(nameof(configOptions));
            }

            return services.AddHttpApi<TInterface>(configOptions)
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
            Action<HttpApiOptions, IServiceProvider> configOptions,
            INacosNamingService svc,
            ILoggerFactory loggerFactory,
            string group = "DEFAULT_GROUP",
            string cluster = "DEFAULT")
            where TInterface : class, IHttpApi
        {
            if (configOptions == null)
            {
                throw new ArgumentNullException(nameof(configOptions));
            }

            return services.AddHttpApi<TInterface>(configOptions)
                    .ConfigurePrimaryHttpMessageHandler(provider =>
                    {
                        return new NacosExtensions.Common.NacosDiscoveryHttpClientHandler(svc, group, cluster, loggerFactory);
                    });
        }
    }
}
