using System;
using System.Linq;
using AspNetCoreRequestTracing.Tests.Server;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AspNetCoreRequestTracing.Tests
{
    /// <summary>
    /// xUnit collection fixture that starts an ASP.NET Core server listening to a random port.
    /// <seealso cref="ServerCollection" />.
    /// </summary>
    public class ServerFixture : IDisposable
    {
        public ServerFixture()
        {
            TestLogger = new TestLogger();
            ServerWebHost = WebHost
                .CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                {
                    builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TestLoggerProvider>((x) => new TestLoggerProvider(TestLogger)));
                    builder.AddFilter<TestLoggerProvider>((category, level) => category.EndsWith(nameof(RequestTracingMiddleware), StringComparison.Ordinal));
                })
                .UseStartup<Startup>()
                .UseUrls("http://127.0.0.1:0")
                .Build();
            ServerWebHost.Start();
        }

        public IWebHost ServerWebHost { get; }

        public TestLogger TestLogger { get; }

        public Uri ServerUri
        {
            get
            {
                var serverAddressesFeature = ServerWebHost.ServerFeatures.Get<IServerAddressesFeature>();
                return new Uri(serverAddressesFeature.Addresses.First());
            }
        }

        public void Dispose()
        {
            if (ServerWebHost != null)
            {
                ServerWebHost.Dispose();
            }
        }
    }
}
