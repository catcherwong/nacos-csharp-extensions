namespace WebApiClient.Extensions.Nacos.Tests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using WebApiClient;
    using WebApiClient.Attributes;

    [HttpHost("http://githubsvc")]
    public interface IGitHubAPI : IHttpApi
    {
        [HttpGet("")]
        Task<HttpResponseMessage> Get();
    }
}
