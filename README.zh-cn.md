# nacos-csharp-extensions                   [English](./README.md)

一些基于 `nacos-sdk-csharp` 的扩展.

## 功能特性

- 声明式服务调用工具集成
    1. WebApiClient
    2. WebApiClientCore
    3. Refit
- 反向代理 [YARP](https://github.com/microsoft/reverse-proxy) 集成
- 其他..


## 基本用法

### API 定义

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

> 注意: 注册上 nacos 的服务名要是小写的！

### 配置

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

### 调用

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