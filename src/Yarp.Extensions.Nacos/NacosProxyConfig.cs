namespace Yarp.Extensions.Nacos
{
    using Microsoft.Extensions.Primitives;
    using System.Collections.Generic;
    using Yarp.ReverseProxy.Configuration;

    internal class NacosProxyConfig : IProxyConfig
    {
        public List<RouteConfig> Routes { get; internal set; } = new List<RouteConfig>();

        public List<ClusterConfig> Clusters { get; internal set; } = new List<ClusterConfig>();

        IReadOnlyList<RouteConfig> IProxyConfig.Routes => Routes;

        IReadOnlyList<ClusterConfig> IProxyConfig.Clusters => Clusters;

        public IChangeToken ChangeToken { get; internal set; } = default!;

        public NacosProxyConfig(List<RouteConfig> routes, List<ClusterConfig> clusters)
        {
            Routes = routes;
            Clusters = clusters;
        }
    }
}