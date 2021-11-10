namespace Refit.Extensions.Nacos
{
    using global::Nacos.V2;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using Refit;

    public static class NacosDiscoveryClientExtensions
    {
        /// <summary>
        /// Add refit with nacos discovery.
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
            where TInterface : class
        {
            return services.AddNacosDiscoveryTypedClient<TInterface>(p => { }, group, cluster);
        }

        /// <summary>
        /// Add refit with nacos discovery.
        /// </summary>
        /// <typeparam name="TInterface">API</typeparam>
        /// <param name="services">services.</param>
        /// <param name="configOptions">The refit config options.</param>
        /// <param name="group">The group name of nacos service.</param>
        /// <param name="cluster">The cluster name of nacos service.</param>
        /// <returns>IHttpClientBuilder</returns>
        public static IHttpClientBuilder AddNacosDiscoveryTypedClient<TInterface>(
            this IServiceCollection services,
            Action<RefitSettings> configOptions,
            string group = "DEFAULT_GROUP",
            string cluster = "DEFAULT")
           where TInterface : class
        {
            NacosExtensions.Common.Guard.NotNull(configOptions, nameof(configOptions));

            var settings = new RefitSettings();
            configOptions.Invoke(settings);

            return services.AddNacosDiscoveryTypedClient<TInterface>(p => settings, group, cluster);
        }

        /// <summary>
        /// Add refit with nacos discovery.
        /// </summary>
        /// <typeparam name="TInterface">API</typeparam>
        /// <param name="services">services.</param>
        /// <param name="configOptions">The refit config options.</param>
        /// <param name="group">The group name of nacos service.</param>
        /// <param name="cluster">The cluster name of nacos service.</param>
        /// <returns>IHttpClientBuilder</returns>
        public static IHttpClientBuilder AddNacosDiscoveryTypedClient<TInterface>(
            this IServiceCollection services,
            Func<IServiceProvider, RefitSettings> configOptions,
            string group = "DEFAULT_GROUP",
            string cluster = "DEFAULT")
           where TInterface : class
        {
            NacosExtensions.Common.Guard.NotNull(configOptions, nameof(configOptions));

            return services
                    .AddRefitClient<TInterface>(configOptions)
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
            Func<IServiceProvider, RefitSettings> configOptions,
            INacosNamingService svc,
            ILoggerFactory loggerFactory,
            string group = "DEFAULT_GROUP",
            string cluster = "DEFAULT")
            where TInterface : class
        {
            NacosExtensions.Common.Guard.NotNull(configOptions, nameof(configOptions));

            return services
                    .AddRefitClient<TInterface>(configOptions)
                    .ConfigurePrimaryHttpMessageHandler(provider =>
                    {
                        return new NacosExtensions.Common.NacosDiscoveryHttpClientHandler(svc, group, cluster, loggerFactory);
                    });
        }
    }
}
