namespace Yarp.Extensions.Nacos.Tests
{
    using global::Nacos.V2.Remote;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using Moq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class DefaultNacosYarpStoreTests
    {
        [Fact]
        public async Task GetServicesAsync_Should_DoPager()
        {
            var options = new NacosYarpOptions
            {
                PreCount = 2,
                GroupNameList = new List<string> { "g" },
            };
            var optionsAccs = Options.Create(options);
            var namingSvc = new Mock<global::Nacos.V2.INacosNamingService>();
            var configMapper = new Mock<INacosYarpConfigMapper>();
            var store = new DefaultNacosYarpStore(NullLoggerFactory.Instance, optionsAccs, namingSvc.Object, configMapper.Object);

            namingSvc.Setup(x => x.GetServicesOfServer(1, 2, "g")).Returns(Task.FromResult(new ListView<string>(3, new List<string> { "a", "b" })));
            namingSvc.Setup(x => x.GetServicesOfServer(2, 2, "g")).Returns(Task.FromResult(new ListView<string>(3, new List<string> { "c" })));

            var result = await store.GetServicesAsync().ConfigureAwait(false);

            Assert.Single(result.Keys);
            Assert.Equal("g", result.Keys.First());
            Assert.Equal(3, result.Values.First().Count);
            Assert.Contains("a", result.Values.First());
            Assert.Contains("b", result.Values.First());
            Assert.Contains("c", result.Values.First());
        }

        [Fact]
        public async Task GetServicesAsync_Should_Not_DoPager()
        {
            var options = new NacosYarpOptions
            {
                PreCount = 2,
                GroupNameList = new List<string> { "g" },
            };
            var optionsAccs = Options.Create(options);
            var namingSvc = new Mock<global::Nacos.V2.INacosNamingService>();
            var configMapper = new Mock<INacosYarpConfigMapper>();
            var store = new DefaultNacosYarpStore(NullLoggerFactory.Instance, optionsAccs, namingSvc.Object, configMapper.Object);

            namingSvc.Setup(x => x.GetServicesOfServer(1, 2, "g")).Returns(Task.FromResult(new ListView<string>(1, new List<string> { "a" })));

            var result = await store.GetServicesAsync().ConfigureAwait(false);

            Assert.Single(result.Keys);
            Assert.Equal("g", result.Keys.First());
            Assert.Single(result.Values.First());
            Assert.Contains("a", result.Values.First());
        }
    }
}