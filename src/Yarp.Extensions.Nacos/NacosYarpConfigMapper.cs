namespace Yarp.Extensions.Nacos
{
    using global::Nacos.V2.Naming.Dtos;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Yarp.ReverseProxy.Configuration;

    internal static class NacosYarpConfigMapper
    {
        private static readonly string HTTP = "http://";
        private static readonly string HTTPS = "https://";
        private static readonly string Secure = "secure";

        internal static RouteConfig BuildRouteConfig(string clusterId, string serviceName)
        {
            // TODO: how do define the path
            // 【/servicename/{**catch-all}】 at first or read from instance metadata?
            return new RouteConfig
            {
                RouteId = $"{clusterId}-route",
                ClusterId = clusterId,
                Match = new RouteMatch { Path = string.Concat("/", serviceName, "/{**catch-all}") },
                Transforms = new List<Dictionary<string, string>> { new Dictionary<string, string> { { "PathRemovePrefix", $"/{serviceName}" } } }
            };
        }

        internal static ClusterConfig BuildClusterConfig(string clusterId, IReadOnlyDictionary<string, DestinationConfig> destinations)
        {
            return new ClusterConfig()
            {
                ClusterId = clusterId,
                Destinations = destinations,
            };
        }

        internal static Dictionary<string, DestinationConfig> BuildDestinationConfig(List<Instance> instances)
        {
            var destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase);

            foreach (var instance in instances)
            {
                var address = instance.Metadata.TryGetValue(Secure, out _)
                     ? $"{HTTPS}{instance.Ip}:{instance.Port}"
                     : $"{HTTP}{instance.Ip}:{instance.Port}";

                var metadata = new ReadOnlyDictionary<string, string>(instance.Metadata
                    .Where(x => x.Key.StartsWith("yarp", StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase));

                var destination = new DestinationConfig
                {
                    Address = address,
                    Metadata = metadata,
                };

                destinations.Add(instance.ServiceName, destination);
            }

            return destinations;
        }
    }
}