namespace Yarp.Extensions.Nacos
{
    using global::Nacos.V2.Naming.Dtos;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Yarp.ReverseProxy.Configuration;

    public class DefaultNacosYarpConfigMapper : INacosYarpConfigMapper
    {
        private static readonly string HTTP = "http://";
        private static readonly string HTTPS = "https://";
        private static readonly string Secure = "secure";
        private static readonly string MetadataPrefix = "yarp";

        public RouteConfig CreateRouteConfig(string clusterId, string serviceName)
        {
            // TODO: how to define the path
            // 【/servicename/{**catch-all}】 at first or read from instance metadata ?
            // what about different group with same service name or ban this one
            return new RouteConfig
            {
                RouteId = $"{clusterId}-route",
                ClusterId = clusterId,
                Match = new RouteMatch { Path = string.Concat("/", serviceName, "/{**catch-all}") },
                Transforms = new List<Dictionary<string, string>> { new Dictionary<string, string> { { "PathRemovePrefix", $"/{serviceName}" } } }
            };
        }

        public ClusterConfig CreateClusterConfig(string clusterId, IReadOnlyDictionary<string, DestinationConfig> destinations)
        {
            return new ClusterConfig()
            {
                ClusterId = clusterId,
                LoadBalancingPolicy = ReverseProxy.LoadBalancing.LoadBalancingPolicies.RoundRobin,
                Destinations = destinations,
            };
        }

        public Dictionary<string, DestinationConfig> CreateDestinationConfig(List<Instance> instances)
        {
            var destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase);

            foreach (var instance in instances)
            {
                var address = instance.Metadata.TryGetValue(Secure, out _)
                     ? $"{HTTPS}{instance.Ip}:{instance.Port}"
                     : $"{HTTP}{instance.Ip}:{instance.Port}";

                // filter the metadata from instance
                var metadata = new ReadOnlyDictionary<string, string>(instance.Metadata
                    .Where(x => x.Key.StartsWith(MetadataPrefix, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase));

                var destination = new DestinationConfig
                {
                    Address = address,
                    Metadata = metadata,
                };

                // TODO: how to define the destination's key, the key should not be changed.
                destinations.Add(address, destination);
            }

            return destinations;
        }
    }
}