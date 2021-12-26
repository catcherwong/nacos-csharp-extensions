namespace Yarp.Extensions.Nacos
{
    using Microsoft.Extensions.Primitives;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Yarp.ReverseProxy.Configuration;

    public interface INacosYarpStore
    {
        /// <summary>
        /// Get the ProxyConfig
        /// </summary>
        /// <returns>ProxyConfig</returns>
        Task<IProxyConfig> GetConfigAsync();

        /// <summary>
        /// Get the ProxyConfig without cache
        /// </summary>
        /// <param name="groupServicesDict">group services dict</param>
        /// <param name="removedService">removed services dict</param>
        /// <returns>ProxyConfig</returns>
        Task<IProxyConfig> GetRealTimeConfigAsync(Dictionary<string, List<string>> groupServicesDict = null, List<string> removedService = null);

        /// <summary>
        /// Reload the config
        /// </summary>
        void Reload();

        /// <summary>
        /// Get reload token
        /// </summary>
        /// <returns>IChangeToken</returns>
        IChangeToken GetReloadToken();

        /// <summary>
        /// Query group services from nacos server
        /// </summary>
        /// <returns>group services dict</returns>
        Task<Dictionary<string, List<string>>> GetServicesAsync();

        /// <summary>
        /// Get cached service (clusterid)
        /// </summary>
        /// <returns>cached services</returns>
        HashSet<string> GetAllServices();
    }
}