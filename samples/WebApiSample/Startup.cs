namespace WebApiSample
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Nacos.AspNetCore.V2;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddNacosAspNet(x =>
            {
                x.ServerAddresses = new List<string> { "http://localhost:8848/" };
                x.EndPoint = "";
                x.Namespace = "cs";
                x.ServiceName = "webapisample";
                x.GroupName = "DEFAULT_GROUP";
                x.ClusterName = "DEFAULT";
                x.PreferredNetworks = "192.168";
                x.Port = 9090;
                x.Weight = 100;
                x.RegisterEnabled = true;
                x.InstanceEnabled = true;
                x.Ephemeral = true;
                x.Secure = false;

                // swich to use http or rpc
                x.NamingUseRpc = true;
            });
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
