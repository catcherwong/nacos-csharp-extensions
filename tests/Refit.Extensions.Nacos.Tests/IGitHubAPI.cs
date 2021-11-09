namespace Refit.Extensions.Nacos.Tests
{
    using System.Net.Http;
    using System.Threading.Tasks;

    [Headers("User-Agent: Refit Nacos Tests")]
    public interface IGitHubAPI
    {
        [Get("")]
        Task<HttpResponseMessage> Get();
    }
}
