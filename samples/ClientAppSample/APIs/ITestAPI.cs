namespace ClientAppSample.APIs
{
    using System.Threading.Tasks;
    using WebApiClient;
    using WebApiClient.Attributes;

    [HttpHost("http://WebApiSample")]
    public interface ITestAPI : IHttpApi
    {
        [HttpGet("values")]
        Task<string> Get();
    }
}
