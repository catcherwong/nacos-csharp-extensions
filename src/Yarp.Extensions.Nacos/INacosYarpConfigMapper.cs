namespace Yarp.Extensions.Nacos
{
    using global::Nacos.V2.Naming.Dtos;
    using System.Collections.Generic;
    using Yarp.ReverseProxy.Configuration;

    public interface INacosYarpConfigMapper
    {
        RouteConfig CreateRouteConfig(string clusterId, string serviceName);

        ClusterConfig CreateClusterConfig(string clusterId, IReadOnlyDictionary<string, DestinationConfig> destinations);

        Dictionary<string, DestinationConfig> CreateDestinationConfig(List<Instance> instances);
    }
}