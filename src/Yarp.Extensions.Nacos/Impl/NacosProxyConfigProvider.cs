namespace Yarp.Extensions.Nacos
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Yarp.ReverseProxy.Configuration;

    public class NacosProxyConfigProvider : BackgroundService, IProxyConfigProvider
    {
        private readonly object _lockObject = new();
        private readonly ILogger _logger;

        private readonly INacosYarpStore _store;
        private readonly NacosYarpOptions _options;
        private NacosProxyConfig _config;
        private CancellationTokenSource _changeToken;
        private bool _disposed;
        private IDisposable _subscription;

        public NacosProxyConfigProvider(
            ILoggerFactory loggerfactory,
            IOptions<NacosYarpOptions> optionsAccs,
            INacosYarpStore store)
        {
            _logger = loggerfactory.CreateLogger<NacosProxyConfigProvider>();
            _options = optionsAccs.Value;
            _store = store;
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _subscription?.Dispose();
                _changeToken?.Dispose();
                _disposed = true;
            }
        }

        public IProxyConfig GetConfig()
        {
            // First time load
            if (_config == null)
            {
                _subscription = ChangeToken.OnChange(_store.GetReloadToken, UpdateConfig);
                UpdateConfig();
            }

            return _config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 1. query the realtime services from nacos server
                // 2. query the cached services
                // 3. has new or removed services ?
                // 4. add new services's cluster and route info
                // 5. reload !!!
                var realTimeServicesDict = await _store.GetServicesAsync().ConfigureAwait(false);
                var realTimeSet = NacosYarpUtils.BuildServiceSet(realTimeServicesDict);
                var realTimeServices = realTimeSet.ToList();

                var cachedServices = _store.GetAllServices().ToList();

                // find new service, should reload.
                var newService = realTimeServices.Except(cachedServices);

                // find removed service, should reload.
                var removedService = cachedServices.Except(realTimeServices);

                if (newService.Any() || removedService.Any())
                {
                    NacosProxyConfig newSnapshot = await _store.GetRealTimeConfigAsync(realTimeServicesDict, removedService.ToList()).ConfigureAwait(false) as NacosProxyConfig;

                    if (newSnapshot != null)
                    {
                        _store.Reload();
                    }
                }
                else
                {
                    _logger?.LogDebug("Nothing was changed!!!");
                }

                await Task.Delay(TimeSpan.FromSeconds(_options.AutoRefreshPeriod), stoppingToken).ConfigureAwait(false);
            }
        }

        private void UpdateConfig()
        {
            // Prevent overlapping updates, especially on startup.
            lock (_lockObject)
            {
                NacosProxyConfig newConfig = null;
                try
                {
                    newConfig = _store.GetConfigAsync().ConfigureAwait(false).GetAwaiter().GetResult() as NacosProxyConfig;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Can not update yarp configuration.");

                    if (_config == null) throw;

                    return;
                }

                var oldToken = _changeToken;
                _changeToken = new CancellationTokenSource();
                newConfig.ChangeToken = new CancellationChangeToken(_changeToken.Token);
                _config = newConfig;

                try
                {
                    oldToken?.Cancel(throwOnFirstException: false);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Cancel old token error");
                }
            }
        }
    }
}