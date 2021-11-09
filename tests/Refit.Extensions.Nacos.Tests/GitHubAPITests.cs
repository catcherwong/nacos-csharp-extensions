namespace Refit.Extensions.Nacos.Tests
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Xunit;
    using Instance = global::Nacos.V2.Naming.Dtos.Instance;

    public class GitHubAPITests
    {
        [Fact]
        public async Task Request_Should_Succeed()
        {
            IServiceCollection services = new ServiceCollection();

            var namingSvc = new Mock<global::Nacos.V2.INacosNamingService>();
            services.AddNacosDiscoveryTypedClient<IGitHubAPI>(
                p => new RefitSettings(),
                namingSvc.Object,
                NullLoggerFactory.Instance, "DEFAULT_GROUP", "DEFAULT")
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri("http://githubsvc");
                    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var api = serviceProvider.GetService<IGitHubAPI>();

            namingSvc.Setup(x => x.SelectOneHealthyInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).Returns(Task.FromResult(BuildInstance()));

            var res = await api.Get().ConfigureAwait(false);
            Assert.Equal(System.Net.HttpStatusCode.OK, res.StatusCode);

            var server = res.Headers.Server.FirstOrDefault()?.Product?.Name;
            Assert.NotNull(server);
            Assert.Equal("Github.com", server, ignoreCase: true);
        }

        private Instance BuildInstance()
        {
            var ins = new Instance
            {
                ServiceName = "githubsvc",
                ClusterName = "DEFAULT",
                Ip = "api.github.com",
                Port = 443,
            };

            ins.AddMetadata("secure", "1");

            return ins;
        }
    }
}
