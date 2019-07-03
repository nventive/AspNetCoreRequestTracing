using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AspNetCoreRequestTracing
{
    /// <summary>
    /// Options for <see cref="RequestTracingMiddleware"/>.
    /// </summary>
    public class RequestTracingMiddlewareOptions
    {
        /// <summary>
        /// Gets or sets the list of Regular expressions used to map the request path for enabling tracing.
        /// If there is no match, tracing is disabled.
        /// </summary>
        public IEnumerable<string> EnableFor { get; set; }
    }
}
