namespace Yarp.Extensions.Nacos.Tests
{
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Generic;
    using Xunit;
    using global::Nacos.V2.DependencyInjection;
    using Yarp.ReverseProxy.Configuration;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Primitives;

    public class ServiceDiscoveryExtensionsTests
    {
        [Fact]
        public void AddNacosServiceDiscovery_Should_Throw_Exception_When_Nacos_Not_Reg()
        {
            IServiceCollection services = new ServiceCollection();

            Assert.Throws<NacosYarpException>(() => services.AddReverseProxy().AddNacosServiceDiscovery());
        }

        [Fact]
        public void AddNacosServiceDiscovery_Should_Succeed_When_Nacos_Reg()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddNacosV2Naming(x =>
            {
                x.Namespace = "test";
                x.ServerAddresses = new List<string>() { "http://localhost:8848" };
            });

            services.AddReverseProxy().AddNacosServiceDiscovery();

            var provider = services.BuildServiceProvider();

            var store = provider.GetRequiredService<INacosYarpStore>();

            Assert.IsType<DefaultNacosYarpStore>(store);
        }

        [Fact]
        public void AddNacosServiceDiscovery_With_Custom_Mapper_Should_Succeed()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddNacosV2Naming(x =>
            {
                x.Namespace = "test";
                x.ServerAddresses = new List<string>() { "http://localhost:8848" };
            });

            services.AddReverseProxy().AddNacosServiceDiscovery();
            services.AddSingleton<INacosYarpConfigMapper, TestConfigMapper>();

            var provider = services.BuildServiceProvider();

            var mapper = provider.GetRequiredService<INacosYarpConfigMapper>();

            Assert.IsType<TestConfigMapper>(mapper);
        }

        [Fact]
        public void AddNacosServiceDiscovery_With_Custom_Sotre_Should_Succeed()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddNacosV2Naming(x =>
            {
                x.Namespace = "test";
                x.ServerAddresses = new List<string>() { "http://localhost:8848" };
            });

            services.AddReverseProxy().AddNacosServiceDiscovery();
            services.AddSingleton<INacosYarpStore, TestStore>();

            var provider = services.BuildServiceProvider();

            var store = provider.GetRequiredService<INacosYarpStore>();

            Assert.IsType<TestStore>(store);
        }

        public class TestStore : INacosYarpStore
        {
            public HashSet<string> GetAllServices() => new HashSet<string>();

            public Task<IProxyConfig> GetConfigAsync() => Task.FromResult((IProxyConfig)new NacosProxyConfig(null, null));

            public Task<IProxyConfig> GetRealTimeConfigAsync(Dictionary<string, List<string>> groupServicesDict = null, List<string> removedService = null)
                => Task.FromResult((IProxyConfig)new NacosProxyConfig(null, null));

            public IChangeToken GetReloadToken() => new NacosYarpReloadToken();

            public Task<Dictionary<string, List<string>>> GetServicesAsync() => Task.FromResult(new Dictionary<string, List<string>>());

            public void Reload()
            {
            }
        }

        public class TestConfigMapper : INacosYarpConfigMapper
        {
            public ClusterConfig CreateClusterConfig(string clusterId, IReadOnlyDictionary<string, DestinationConfig> destinations)
            {
                return new ClusterConfig { };
            }

            public Dictionary<string, DestinationConfig> CreateDestinationConfig(List<global::Nacos.V2.Naming.Dtos.Instance> instances)
            {
                return new Dictionary<string, DestinationConfig>();
            }

            public RouteConfig CreateRouteConfig(string clusterId, string serviceName)
            {
                return new RouteConfig { };
            }
        }
    }
}