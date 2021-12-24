namespace Yarp.Extensions.Nacos
{
    using Microsoft.Extensions.Primitives;
    using System.Threading.Tasks;
    using Yarp.ReverseProxy.Configuration;

    public interface INacosYarpStore
    {
        Task<IProxyConfig> GetConfigAsync();

        Task<IProxyConfig> GetRealTimeConfigAsync();

        void Reload();

        IChangeToken GetReloadToken();
    }
}