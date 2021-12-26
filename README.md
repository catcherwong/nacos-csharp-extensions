# nacos-csharp-extensions                   [中文](./README.zh-cn.md)

Some extensions for `nacos-sdk-csharp`.

## Features

- Declarative REST Client Integration
    1. WebApiClient Integration
    2. WebApiClientCore Integration
    3. Refit Integration
- Reverse proxy [YARP](https://github.com/microsoft/reverse-proxy) Integration
- Others..


## Basic Usage

### API Definition

```cs
// for webapiclient/core
[HttpHost("http://githubsvc")]
public interface IGitHubAPI : IHttpApi
{
    [HttpGet("")]
    Task<string> Get();
}

// for refit
[Headers("User-Agent: Refit Nacos Tests")]
public interface IGitHubAPI
{
    [Get("")]
    Task<HttpResponseMessage> Get();
}
```

> NOTE: The service name must be lowercase!

### Config

```cs
// nacos dependency
services.AddNacosV2Naming(x =>
{
});

// for webapiclient/core
services.AddNacosDiscoveryTypedClient<IGitHubAPI>("DEFAULT_GROUP", "DEFAULT");

// for refit
services.AddNacosDiscoveryTypedClient<IGitHubAPI>("DEFAULT_GROUP", "DEFAULT")
        .ConfigureHttpClient(c =>
        {
            // The service name must be lowercase!
            c.BaseAddress = new Uri("http://githubsvc");
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
```

### Call

```cs
[ApiController]
[Route("[controller]")]
public class ValuesController : ControllerBase
{
    private readonly IGitHubAPI _api;

    public ValuesController(IGitHubAPI api)
    {
        this._api = api;
    }

    [HttpGet]
    public async Task<string> Get()
    {
        var res = await _api.Get().ConfigureAwait(false);

        return $"{res} from IGitHubAPI";
    }
}
```