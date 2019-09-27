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
#if NETCOREAPP2_2
            services.AddMvc();
#endif

#if NETCOREAPP3_0
            services.AddControllers();
#endif
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRequestTracing();
#if NETCOREAPP2_2
            app.UseMvc();
#endif
#if NETCOREAPP3_0
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
#endif
        }
    }
}
