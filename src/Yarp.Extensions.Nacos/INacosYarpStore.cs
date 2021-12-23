namespace Yarp.Extensions.Nacos
{
    using Yarp.ReverseProxy.Configuration;
    using Microsoft.Extensions.Primitives;
    using System.Threading.Tasks;

    public interface INacosYarpStore
    {
        Task<IProxyConfig> GetConfigAsync();

        void Reload();

        IChangeToken GetReloadToken();
    }
}