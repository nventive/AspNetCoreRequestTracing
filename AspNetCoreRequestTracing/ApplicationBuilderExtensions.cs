using System;
using Microsoft.AspNetCore.Builder;

namespace AspNetCoreRequestTracing
{
    /// <summary>
    /// <see cref="IApplicationBuilder"/> extension methods.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables Full Request/Response Tracing.
        /// Be careful to only enable it when you need it, as there is a serious impact on performance and throughput,
        /// as requests and responses need to be buffered to allow tracing.
        /// It can be selectively enabled using <see cref="RequestTracingMiddlewareOptions.EnableFor"/> for specific path.
        /// You must configure the <see cref="RequestTracingMiddlewareOptions"/> to use this feature.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The configured <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseRequestTracing(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestTracingMiddleware>();
        }
    }
}
