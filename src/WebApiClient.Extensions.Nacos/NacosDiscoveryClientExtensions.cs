namespace WebApiClient.Extensions.Nacos
{
    using global::Nacos.V2;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using WebApiClient.Extensions.HttpClientFactory;

    public static class NacosDiscoveryClientExtensions
    {
        public static IHttpClientBuilder AddNacosDiscoveryTypedClient<TInterface>(this IServiceCollection services, string group = "DEFAULT_GROUP", string cluster = "DEFAULT")
            where TInterface : class, IHttpApi
        {
            return services.AddNacosDiscoveryTypedClient<TInterface>(c => { }, group, cluster);
        }

        public static IHttpClientBuilder AddNacosDiscoveryTypedClient<TInterface>(this IServiceCollection services, Action<HttpApiConfig> configOptions, string group = "DEFAULT_GROUP", string cluster = "DEFAULT")
            where TInterface : class, IHttpApi
        {
            if (configOptions == null)
            {
                throw new ArgumentNullException(nameof(configOptions));
            }

            return services.AddNacosDiscoveryTypedClient<TInterface>((c, p) => configOptions.Invoke(c), group, cluster);
        }


        public static IHttpClientBuilder AddNacosDiscoveryTypedClient<TInterface>(this IServiceCollection services, Action<HttpApiConfig, IServiceProvider> configOptions, string group = "DEFAULT_GROUP", string cluster = "DEFAULT")
            where TInterface : class, IHttpApi
        {
            if (configOptions == null)
            {
                throw new ArgumentNullException(nameof(configOptions));
            }

            return services
                    .AddHttpApiTypedClient<TInterface>(configOptions)
                    .ConfigurePrimaryHttpMessageHandler(provider =>
                    {
                        var svc = provider.GetRequiredService<INacosNamingService>();
                        var loggerFactory = provider.GetService<ILoggerFactory>();

                        if (svc == null)
                        {
                            throw new InvalidOperationException(
                                "Can not find out INacosNamingService, please register at first");
                        }

                        return new NacosDiscoveryHttpClientHandler(svc, group, cluster, loggerFactory);
                    });
        }
    }
}
