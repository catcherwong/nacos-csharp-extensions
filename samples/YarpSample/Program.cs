using global::Nacos.V2.DependencyInjection;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNacosV2Naming(x =>
{
    x.ServerAddresses = new System.Collections.Generic.List<string> { "http://localhost:8848/" };
    x.Namespace = "cs";

    // swich to use http or rpc
    x.NamingUseRpc = true;
});

builder.Services.AddReverseProxy()
    .AddNacosServiceDiscovery();

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/yarp", (IProxyConfigProvider provider) =>
    {
        var res = provider.GetConfig();
        return Results.Ok(res);
    });
    endpoints.MapReverseProxy();
});

app.Run("http://*:9091");
