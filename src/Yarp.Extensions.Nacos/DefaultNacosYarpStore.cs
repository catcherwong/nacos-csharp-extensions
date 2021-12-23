namespace Yarp.Extensions.Nacos
{
    using Yarp.ReverseProxy.Configuration;
    using Microsoft.Extensions.Primitives;
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using global::Nacos.V2.Naming.Dtos;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Logging;
    using System.Collections.Concurrent;
    using Microsoft.Extensions.DependencyInjection;
    using global::Nacos.V2;

    public class DefaultNacosYarpStore : INacosYarpStore
    {
        private NacosYarpReloadToken _reloadToken = new NacosYarpReloadToken();
        private readonly ILogger _logger;
        private readonly NacosYarpOptions _options;
        private readonly global::Nacos.V2.INacosNamingService _nameSvc;
        private readonly HashSet<string> _cachedServices = new HashSet<string>();
        private readonly ConcurrentDictionary<string, RouteConfig> _cachedRoutes = new ConcurrentDictionary<string, RouteConfig>();
        private readonly ConcurrentDictionary<string, ClusterConfig> _cachedClusters = new ConcurrentDictionary<string, ClusterConfig>();
        private readonly ServiceChangeEventListener _listener;

        public DefaultNacosYarpStore(ILoggerFactory loggerFactory, IOptions<NacosYarpOptions> optionsAccs, INacosNamingService nameSvc)
        {
            _logger = loggerFactory.CreateLogger<DefaultNacosYarpStore>();
            _options = optionsAccs.Value;
            _nameSvc = nameSvc;
            _listener = new ServiceChangeEventListener(this);
        }

        public IChangeToken GetReloadToken() => _reloadToken;

        public void Reload()
        {
            Interlocked.Exchange(ref this._reloadToken, new NacosYarpReloadToken()).OnReload();
        }

        public async Task<IProxyConfig> GetConfigAsync()
        {
            NacosProxyConfig snapshot = null;

            if (_cachedClusters.Any() && _cachedRoutes.Any())
            {
                snapshot = new NacosProxyConfig(_cachedRoutes.Values.ToList(), _cachedClusters.Values.ToList());
                return snapshot;
            }

            try
            {
                var list = new List<string>();

                if (!_options.GroupList.Any())
                {
                    list.Add(global::Nacos.V2.Common.Constants.DEFAULT_GROUP);
                }
                else
                {
                    list.AddRange(_options.GroupList);
                }

                foreach (var groupName in list)
                {
                    // TODO: more than PreCount services, pager here
                    var listView = await _nameSvc.GetServicesOfServer(1, _options.PreCount, groupName).ConfigureAwait(false);

                    if (listView.Count > 0)
                    {
                        var clusters = new Dictionary<string, ClusterConfig>();
                        var routes = new Dictionary<string, RouteConfig>();

                        foreach (var serviceName in listView.Data)
                        {
                            var instances = await _nameSvc.GetAllInstances(serviceName, groupName, false).ConfigureAwait(false);
                            _ = Task.Run(async () => await _nameSvc.Subscribe(serviceName, groupName, _listener).ConfigureAwait(false));

                            // group + service = unique
                            var clusterId = $"{groupName}@@{serviceName}";
                            _cachedServices.Add(clusterId);

                            // ClusterConfig
                            var cluster = NacosYarpConfigMapper.BuildClusterConfig(clusterId, NacosYarpConfigMapper.BuildDestinationConfig(instances));
                            clusters[clusterId] = cluster;

                            // RouteConfig
                            var route = NacosYarpConfigMapper.BuildRouteConfig(clusterId, serviceName);
                            routes[clusterId] = route;
                        }

                        snapshot = new NacosProxyConfig(routes.Values.ToList(), clusters.Values.ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Query service from Nacos server error");
            }

            return snapshot;
        }

        internal sealed class ServiceChangeEventListener : IEventListener
        {
            private readonly DefaultNacosYarpStore _store;

            public ServiceChangeEventListener(DefaultNacosYarpStore store)
            {
                _store = store;
            }

            public async Task OnEvent(IEvent @event)
            {
                if (@event is global::Nacos.V2.Naming.Event.InstancesChangeEvent e)
                {
                    // group + service = unique
                    var clusterId = $"{e.GroupName}@@{e.ServiceName}";

                    if (_store._cachedClusters.TryGetValue(clusterId, out var oldCluster) && e.Hosts.Count > 0)
                    {
                        var instances = await _store._nameSvc.GetAllInstances(e.ServiceName, e.GroupName, false).ConfigureAwait(false);

                        // ClusterConfig
                        var newCluster = NacosYarpConfigMapper.BuildClusterConfig(clusterId, NacosYarpConfigMapper.BuildDestinationConfig(instances));

                        _store._cachedClusters[clusterId] = newCluster;

                        // var config = new NacosProxyConfig(_store._cachedRoutes.Values.ToList(), _store._cachedClusters.Values.ToList());
                        Console.WriteLine("IEventListener-----update");
                        _store.Reload();
                    }
                }
            }
        }
    }
}