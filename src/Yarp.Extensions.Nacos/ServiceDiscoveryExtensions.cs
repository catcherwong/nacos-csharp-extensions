namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System.Linq;
    using Yarp.Extensions.Nacos;
    using Yarp.ReverseProxy.Configuration;

    public static class ServiceDiscoveryExtensions
    {
        /// <summary>
        /// Add Nacos service discovery support
        /// </summary>
        /// <param name="builder">builder</param>
        /// <param name="groupNames">group name list split by ,</param>
        /// <param name="percount">per count</param>
        /// <param name="enableAutoRefreshService">enable auto refresh service</param>
        /// <param name="autoRefreshPeriod">auto refresh period</param>
        /// <returns>IReverseProxyBuilder</returns>
        /// <exception cref="NacosYarpException"></exception>
        public static IReverseProxyBuilder AddNacosServiceDiscovery(
            this IReverseProxyBuilder builder,
            string groupNames = "DEFAULT_GROUP",
            int percount = 50,
            bool enableAutoRefreshService = true,
            int autoRefreshPeriod = 600)
        {
            var nacosNamingSvc = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(Nacos.V2.INacosNamingService));
            if (nacosNamingSvc == null) throw new NacosYarpException("Please register INacosNamingService at first");

            builder.Services.Configure<NacosYarpOptions>(x =>
            {
                x.GroupNameList = groupNames.Split(',').ToList();
                x.PreCount = percount;
                x.EnableAutoRefreshService = enableAutoRefreshService;
                x.AutoRefreshPeriod = autoRefreshPeriod;
            });

            builder.Services.TryAddSingleton<INacosYarpStore, DefaultNacosYarpStore>();
            builder.Services.TryAddSingleton<INacosYarpConfigMapper, DefaultNacosYarpConfigMapper>();
            builder.Services.AddSingleton<IProxyConfigProvider, NacosProxyConfigProvider>();

            if (enableAutoRefreshService)
            {
                builder.Services.AddHostedService<NacosProxyConfigProvider>();
            }

            return builder;
        }

        public static IReverseProxyBuilder AddNacosServiceDiscovery(this IReverseProxyBuilder builder, IConfiguration configuration, string sectionName = "yarp:nacos")
        {
            var nacosNamingSvc = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(Nacos.V2.INacosNamingService));
            if (nacosNamingSvc == null) throw new NacosYarpException("Please register INacosNamingService at first");

            var configSection = configuration.GetSection(sectionName);

            var options = new NacosYarpOptions();
            configSection.Bind(options);

            builder.Services.TryAddSingleton<INacosYarpStore, DefaultNacosYarpStore>();
            builder.Services.TryAddSingleton<INacosYarpConfigMapper, DefaultNacosYarpConfigMapper>();
            builder.Services.AddSingleton<IProxyConfigProvider, NacosProxyConfigProvider>();

            if (options.EnableAutoRefreshService)
            {
                builder.Services.AddHostedService<NacosProxyConfigProvider>();
            }

            return builder;
        }
    }
}