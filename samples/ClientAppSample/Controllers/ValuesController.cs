namespace ClientAppSample.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [ApiController]
    [Route("[controller]")]
    public class ValuesController : ControllerBase
    {
        private readonly APIs.ITestAPI _testAPI;

        public ValuesController(APIs.ITestAPI testAPI)
        {
            this._testAPI = testAPI;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            var res = await _testAPI.Get().ConfigureAwait(false);

            return $"{res} from ITestAPI";
        }
    }
}
