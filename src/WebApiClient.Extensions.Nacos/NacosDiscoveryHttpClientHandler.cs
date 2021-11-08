namespace WebApiClient.Extensions.Nacos
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using INacosNamingService = global::Nacos.V2.INacosNamingService;
    using NacosConstants = global::Nacos.V2.Common.Constants;

    public class NacosDiscoveryHttpClientHandler : HttpClientHandler
    {
        private readonly ILogger _logger;
        private readonly INacosNamingService _namingService;
        private string _groupName = string.Empty;
        private string _cluster = string.Empty;

        public NacosDiscoveryHttpClientHandler(INacosNamingService namingService, string group = null, string cluster = null, ILoggerFactory loggerFactory = null)
        {
            _namingService = namingService;

            _groupName = group ?? NacosConstants.DEFAULT_GROUP;
            _cluster = cluster ?? NacosConstants.DEFAULT_CLUSTER_NAME;

            _logger = loggerFactory?.CreateLogger<NacosDiscoveryHttpClientHandler>();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var current = request.RequestUri;
            try
            {
                request.RequestUri = await LookupServiceAsync(current).ConfigureAwait(false);
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger?.LogDebug(e, "Exception during SendAsync()");
                throw;
            }
            finally
            {
                request.RequestUri = current;
            }
        }

        internal async Task<Uri> LookupServiceAsync(Uri request)
        {
            var instance = await _namingService.SelectOneHealthyInstance(request.Host, _groupName, new List<string> { _cluster }, true).ConfigureAwait(false);

            if (instance != null)
            {
                var host = $"{instance.Ip}:{instance.Port}";

                var baseUrl = instance.Metadata.TryGetValue(Secure, out _)
                    ? $"{HTTPS}{host}"
                    : $"{HTTP}{host}";

                var uriBase = new Uri(baseUrl);
                return new Uri(uriBase, request.PathAndQuery);
            }

            return request;
        }

        private static readonly string HTTP = "http://";
        private static readonly string HTTPS = "https://";
        private static readonly string Secure = "secure";
    }
}
