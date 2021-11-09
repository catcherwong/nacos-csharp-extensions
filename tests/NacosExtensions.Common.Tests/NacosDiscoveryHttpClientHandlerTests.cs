namespace WebApiClient.Extensions.Nacos.Tests
{
    using Moq;
    using NacosExtensions.Common;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;
    using INacosNamingService = global::Nacos.V2.INacosNamingService;
    using Instance = global::Nacos.V2.Naming.Dtos.Instance;

    public class NacosDiscoveryHttpClientHandlerTests
    {
        private NacosDiscoveryHttpClientHandler _handler;
        private Mock<INacosNamingService> _namingSvc;

        public NacosDiscoveryHttpClientHandlerTests()
        {
            _namingSvc = new Mock<global::Nacos.V2.INacosNamingService>();

            _handler = new NacosDiscoveryHttpClientHandler(_namingSvc.Object);
        }

        [Fact]
        public async Task LookupServiceAsync_When_Without_Secure_Should_Succeed()
        {
            var uri = new System.Uri("http://testsvc/api/values");

            _namingSvc.Setup(x => x.SelectOneHealthyInstance("testsvc", It.IsAny<string>(), It.IsAny<List<string>>(), true)).Returns(Task.FromResult(BuildInstance()));

            var res = await _handler.LookupServiceAsync(uri).ConfigureAwait(false);

            Assert.Equal("http://192.168.1.101:9020/api/values", res.AbsoluteUri);
        }

        [Fact]
        public async Task LookupServiceAsync_When_With_Secure_Should_Succeed()
        {
            var uri = new System.Uri("http://testsvc/api/values");

            _namingSvc.Setup(x => x.SelectOneHealthyInstance("testsvc", It.IsAny<string>(), It.IsAny<List<string>>(), true)).Returns(Task.FromResult(BuildInstance(true)));

            var res = await _handler.LookupServiceAsync(uri).ConfigureAwait(false);

            Assert.Equal("https://192.168.1.101:9020/api/values", res.AbsoluteUri);
        }

        [Fact]
        public async Task LookupServiceAsync_When_Return_Null_Should_Be_Raw()
        {
            var uri = new System.Uri("http://testsvc/api/values");

            _namingSvc.Setup(x => x.SelectOneHealthyInstance("testsvc", It.IsAny<string>(), It.IsAny<List<string>>(), true)).Returns(Task.FromResult<Instance>(null));

            var res = await _handler.LookupServiceAsync(uri).ConfigureAwait(false);

            Assert.Equal("http://testsvc/api/values", res.AbsoluteUri);
        }

        private Instance BuildInstance(bool isSecure = false)
        {
            var ins = new Instance
            {
                ServiceName = "TESTSVC",
                ClusterName = "DEFAULT",
                Ip = "192.168.1.101",
                Port = 9020,
            };

            if (isSecure)
            {
                ins.AddMetadata("secure", "1");
            }

            return ins;
        }
    }
}
