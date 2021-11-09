namespace WebApiClientCore.Extensions.Nacos.Tests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using WebApiClientCore.Attributes;

    [HttpHost("http://githubsvc")]
    public interface IGitHubAPI : IHttpApi
    {
        [HttpGet("")]
        Task<HttpResponseMessage> Get();
    }
}
