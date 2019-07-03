using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreRequestTracing.Tests.Server
{
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Default ASP.Net Core startup class.")]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RequestTracingMiddlewareOptions>(options =>
            {
                options.EnableFor = new[] { ".*" };
            });
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRequestTracing();
            app.UseMvc();
        }
    }
}
