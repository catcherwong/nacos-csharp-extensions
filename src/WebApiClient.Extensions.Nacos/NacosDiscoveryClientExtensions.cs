namespace WebApiClient.Extensions.Nacos
{
    using global::Nacos.V2;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using WebApiClient.Extensions.HttpClientFactory;

    public static class NacosDiscoveryClientExtensions
    {
        /// <summary>
        /// Add webapiclient with nacos discovery.
        /// </summary>
        /// <typeparam name="TInterface">API</typeparam>
        /// <param name="services">services.</param>
        /// <param name="group">The group name of nacos service.</param>
        /// <param name="cluster">The cluster name of nacos service.</param>
        /// <returns>IHttpClientBuilder</returns>
        public static IHttpClientBuilder AddNacosDiscoveryTypedClient<TInterface>(
            this IServiceCollection services,
            string group = "DEFAULT_GROUP",
            string cluster = "DEFAULT")
            where TInterface : class, IHttpApi
        {
            return services.AddNacosDiscoveryTypedClient<TInterface>(c => { }, group, cluster);
        }

        /// <summary>
        /// Add webapiclient with nacos discovery.
        /// </summary>
        /// <typeparam name="TInterface">API</typeparam>
        /// <param name="services">services.</param>
        /// <param name="configOptions">The webapiclient config options.</param>
        /// <param name="group">The group name of nacos service.</param>
        /// <param name="cluster">The cluster name of nacos service.</param>
        /// <returns>IHttpClientBuilder</returns>
        public static IHttpClientBuilder AddNacosDiscoveryTypedClient<TInterface>(
            this IServiceCollection services,
            Action<HttpApiConfig> configOptions,
            string group = "DEFAULT_GROUP",
            string cluster = "DEFAULT")
            where TInterface : class, IHttpApi
        {
            NacosExtensions.Common.Guard.NotNull(configOptions, nameof(configOptions));

            return services.AddNacosDiscoveryTypedClient<TInterface>((c, p) => configOptions.Invoke(c), group, cluster);
        }

        /// <summary>
        /// Add webapiclient with nacos discovery.
        /// </summary>
        /// <typeparam name="TInterface">API</typeparam>
        /// <param name="services">services.</param>
        /// <param name="configOptions">The webapiclient config options.</param>
        /// <param name="group">The group name of nacos service.</param>
        /// <param name="cluster">The cluster name of nacos service.</param>
        /// <returns>IHttpClientBuilder</returns>
        public static IHttpClientBuilder AddNacosDiscoveryTypedClient<TInterface>(
            this IServiceCollection services,
            Action<HttpApiConfig, IServiceProvider> configOptions,
            string group = "DEFAULT_GROUP",
            string cluster = "DEFAULT")
            where TInterface : class, IHttpApi
        {
            NacosExtensions.Common.Guard.NotNull(configOptions, nameof(configOptions));

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

                        return new NacosExtensions.Common.NacosDiscoveryHttpClientHandler(svc, group, cluster, loggerFactory);
                    });
        }

        // fot test
        internal static IHttpClientBuilder AddNacosDiscoveryTypedClient<TInterface>(
            this IServiceCollection services,
            Action<HttpApiConfig, IServiceProvider> configOptions,
            INacosNamingService svc,
            ILoggerFactory loggerFactory,
            string group = "DEFAULT_GROUP",
            string cluster = "DEFAULT")
            where TInterface : class, IHttpApi
        {
            NacosExtensions.Common.Guard.NotNull(configOptions, nameof(configOptions));

            return services
                    .AddHttpApiTypedClient<TInterface>(configOptions)
                    .ConfigurePrimaryHttpMessageHandler(provider =>
                    {
                        return new NacosExtensions.Common.NacosDiscoveryHttpClientHandler(svc, group, cluster, loggerFactory);
                    });
        }
    }
}
