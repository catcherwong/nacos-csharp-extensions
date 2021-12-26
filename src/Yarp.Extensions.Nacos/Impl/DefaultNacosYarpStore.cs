namespace Yarp.Extensions.Nacos
{
    using global::Nacos.V2;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Yarp.ReverseProxy.Configuration;

    public class DefaultNacosYarpStore : INacosYarpStore
    {
        private NacosYarpReloadToken _reloadToken = new();
        private readonly ILogger _logger;
        private readonly NacosYarpOptions _options;
        private readonly INacosNamingService _nameSvc;
        private readonly INacosYarpConfigMapper _configMapper;
        private readonly HashSet<string> _cachedServices = new();
        private readonly ConcurrentDictionary<string, RouteConfig> _cachedRoutes = new();
        private readonly ConcurrentDictionary<string, ClusterConfig> _cachedClusters = new();
        private readonly ServiceChangeEventListener _listener;

        public DefaultNacosYarpStore(
            ILoggerFactory loggerFactory,
            IOptions<NacosYarpOptions> optionsAccs,
            INacosNamingService nameSvc,
            INacosYarpConfigMapper configMapper)
        {
            _logger = loggerFactory.CreateLogger<DefaultNacosYarpStore>();
            _options = optionsAccs.Value;
            _nameSvc = nameSvc;
            _configMapper = configMapper;
            _listener = new ServiceChangeEventListener(_logger, this);
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
            }
            else
            {
                snapshot = await GetRealTimeConfigAsync().ConfigureAwait(false) as NacosProxyConfig;
            }

            if (snapshot == null) throw new NacosYarpException("Can not get the yarp config!!");

            return snapshot;
        }


        public async Task<IProxyConfig> GetRealTimeConfigAsync(Dictionary<string, List<string>> groupServicesDict = null, List<string> removedService = null)
        {
            NacosProxyConfig snapshot = null;

            if (groupServicesDict == null || !groupServicesDict.Any())
            {
                groupServicesDict = await GetServicesAsync().ConfigureAwait(false);
            }

            if (removedService != null && removedService.Any())
            {
                foreach (var item in removedService)
                {
                    _cachedServices.Remove(item);
                    _cachedClusters.Remove(item, out _);
                    _cachedRoutes.Remove(item, out _);

                    // TODO: should unsubscribe or not ?
                    var arr = item.Split("@@");
                    _ = Task.Run(async () => await _nameSvc.Unsubscribe(arr[1], arr[0], _listener).ConfigureAwait(false));

                    _logger?.LogDebug("removed {service} from yarp", item);
                }
            }

            var clusters = new Dictionary<string, ClusterConfig>();
            var routes = new Dictionary<string, RouteConfig>();

            foreach (var item in groupServicesDict)
            {
                var groupName = item.Key;
                var groupServices = item.Value;

                foreach (var serviceName in groupServices)
                {
                    try
                    {
                        var instances = await _nameSvc.GetAllInstances(serviceName, groupName, false).ConfigureAwait(false);
                        _ = Task.Run(async () => await _nameSvc.Subscribe(serviceName, groupName, _listener).ConfigureAwait(false));

                        // group + service = unique
                        var clusterId = NacosYarpUtils.CreateClusterId(groupName, serviceName);
                        _cachedServices.Add(clusterId);

                        // ClusterConfig
                        var cluster = _configMapper.CreateClusterConfig(clusterId, _configMapper.CreateDestinationConfig(instances));
                        clusters[clusterId] = cluster;
                        _cachedClusters[clusterId] = cluster;

                        // RouteConfig
                        var route = _configMapper.CreateRouteConfig(clusterId, serviceName);
                        routes[clusterId] = route;
                        _cachedRoutes[clusterId] = route;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Build yarp config fail, serviceName={serviceName}, groupName={groupName}", serviceName, groupName);
                    }
                }
            }

            snapshot = new NacosProxyConfig(routes.Values.ToList(), clusters.Values.ToList());

            return snapshot;
        }

        public async Task<Dictionary<string, List<string>>> GetServicesAsync()
        {
            Dictionary<string, List<string>> groupServicesDict = new();

            var groupNamelist = new List<string>();

            if (!_options.GroupNameList.Any())
            {
                groupNamelist.Add(global::Nacos.V2.Common.Constants.DEFAULT_GROUP);
            }
            else
            {
                groupNamelist.AddRange(_options.GroupNameList);
            }

            foreach (var groupName in groupNamelist)
            {
                try
                {
                    int pageNo = 1;

                    // There are some bug for【nacos 2.0.3】 with grpc, it do not return the correct count.
                    // Should wait for the next release for nacos server.
                    // To avoid this issue, set the precount to a larger value
                    var listView = await _nameSvc.GetServicesOfServer(pageNo, _options.PreCount, groupName).ConfigureAwait(false);

                    var total = listView.Count;

                    // do not need to do pager here
                    if (total > 0 && total < _options.PreCount)
                    {
                        var groupServices = listView.Data;
                        groupServicesDict.Add(groupName, groupServices);
                    }

                    // need to do pager here
                    if (total > _options.PreCount)
                    {
                        var groupServices = new List<string>();
                        groupServices.AddRange(listView.Data);

                        do
                        {
                            pageNo++;
                            var tmp = await _nameSvc.GetServicesOfServer(pageNo, _options.PreCount, groupName).ConfigureAwait(false);
                            groupServices.AddRange(tmp.Data);
                        }
                        while (total > _options.PreCount * pageNo);

                        groupServicesDict.Add(groupName, groupServices);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Load services from {groupName} fail.", groupName);
                }
            }

            return groupServicesDict;
        }

        public HashSet<string> GetAllServices() => _cachedServices;

        internal sealed class ServiceChangeEventListener : IEventListener
        {
            private readonly ILogger _logger;
            private readonly DefaultNacosYarpStore _store;

            public ServiceChangeEventListener(ILogger logger, DefaultNacosYarpStore store)
            {
                _logger = logger;
                _store = store;
            }

            public async Task OnEvent(IEvent @event)
            {
                if (@event is global::Nacos.V2.Naming.Event.InstancesChangeEvent e)
                {
                    // group + service = unique
                    var clusterId = NacosYarpUtils.CreateClusterId(e.GroupName, e.ServiceName);

                    // all of the hosts of a service are down, listener will do nothing here
                    if (_store._cachedClusters.TryGetValue(clusterId, out var oldCluster) && e.Hosts.Count > 0)
                    {
                        _logger?.LogDebug("Before update yarp cluster config, serviceName={serviceName}, groupName={groupName}, config={config}", e.ServiceName, e.GroupName, oldCluster.ToString());

                        try
                        {
                            // find newest instances from nacos server.
                            var instances = await _store._nameSvc.GetAllInstances(e.ServiceName, e.GroupName, false).ConfigureAwait(false);

                            // ClusterConfig
                            var newCluster = _store._configMapper.CreateClusterConfig(clusterId, _store._configMapper.CreateDestinationConfig(instances));

                            _store._cachedClusters[clusterId] = newCluster;

                            _logger?.LogDebug("After update yarp cluster config, serviceName={serviceName}, groupName={groupName}, config={config}", e.ServiceName, e.GroupName, newCluster.ToString());

                            _store.Reload();
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, "Update yarp clusters config fail, serviceName={serviceName}, groupName={groupName}", e.ServiceName, e.GroupName);
                        }
                    }
                }
            }
        }
    }
}