namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System.Linq;
    using Yarp.Extensions.Nacos;
    using Yarp.ReverseProxy.Configuration;

    public static class ServiceDiscoveryExtensions
    {
        public static IReverseProxyBuilder AddNacosServiceDiscovery(
            this IReverseProxyBuilder builder,
            string groupName = "DEFAULT_GROUP",
            int percount = 50,
            bool enableAutoRefreshService = true,
            int autoRefreshPeriod = 60)
        {
            var nacosNamingSvc = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(Nacos.V2.INacosNamingService));
            if (nacosNamingSvc == null) throw new NacosYarpException("Please reg INacosNamingService at first");

            builder.Services.Configure<NacosYarpOptions>(x =>
            {
                x.GroupList = groupName.Split(',').ToList();
                x.PreCount = percount;
                x.EnableAutoRefreshService = enableAutoRefreshService;
                x.AutoRefreshPeriod = autoRefreshPeriod;
            });

            builder.Services.TryAddSingleton<INacosYarpStore, DefaultNacosYarpStore>();
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
            if (nacosNamingSvc == null) throw new NacosYarpException("Please reg INacosNamingService at first");

            var configSection = configuration.GetSection(sectionName);

            var options = new NacosYarpOptions();
            configSection.Bind(options);

            builder.Services.TryAddSingleton<INacosYarpStore, DefaultNacosYarpStore>();
            builder.Services.AddSingleton<IProxyConfigProvider, NacosProxyConfigProvider>();

            if (options.EnableAutoRefreshService)
            {
                builder.Services.AddHostedService<NacosProxyConfigProvider>();
            }

            return builder;
        }
    }
}