using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
#if NETCOREAPP2_1
using Microsoft.AspNetCore.Http.Internal;
#endif
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;

namespace AspNetCoreRequestTracing
{
    /// <summary>
    /// This middleware is responsible for full request/response tracing.
    /// Be careful to only enable it when you need it, as there is a serious impact on performance and throughput,
    /// as requests and responses need to be buffered to allow tracing.
    /// It can be selectively enabled using <see cref="RequestTracingMiddlewareOptions.EnableFor"/> for specific path.
    /// </summary>
    public class RequestTracingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly RecyclableMemoryStreamManager _memoryStreamManager = new RecyclableMemoryStreamManager();

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestTracingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next <see cref="RequestDelegate"/> in the chain.</param>
        /// <param name="logger">The <see cref="ILogger"/> to use.</param>
        public RequestTracingMiddleware(
            RequestDelegate next,
            ILogger<RequestTracingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Invoked by ASP.NET.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="options">The current <see cref="RequestTracingMiddlewareOptions"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Invoke(HttpContext context, IOptions<RequestTracingMiddlewareOptions> options)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (options == null || options.Value.EnableFor == null || !options.Value.EnableFor.Any() || !IsMatch(options.Value.EnableFor, context.Request))
            {
                await _next(context);
                return;
            }

            // First, we prep the context for enabling replay.
#if NETCOREAPP2_1
            context.Request.EnableRewind();
#endif
#if NETCOREAPP3_0
            context.Request.EnableBuffering();
#endif

            // Then we split the response stream into 2 parallel streams and allow the buffering of the response.
            using (var inMemorySecondaryStream = _memoryStreamManager.GetStream())
            {
                var duplicateStream = new DuplicateStream(context.Response.Body, inMemorySecondaryStream);
                context.Response.Body = duplicateStream;

                await _logger.RequestTrace(context.Request);

                await _next(context);

                if (context.Response.StatusCode < StatusCodes.Status400BadRequest)
                {
                    await _logger.ResponseTrace(context.Response, inMemorySecondaryStream, context.Request.Protocol);
                }

                if (context.Response.StatusCode >= StatusCodes.Status400BadRequest && context.Response.StatusCode < StatusCodes.Status500InternalServerError)
                {
                    await _logger.ResponseWarning(context.Response, inMemorySecondaryStream, context.Request.Protocol);
                }

                if (context.Response.StatusCode >= StatusCodes.Status500InternalServerError)
                {
                    await _logger.ResponseError(context.Response, inMemorySecondaryStream, context.Request.Protocol);
                }
            }
        }

        private bool IsMatch(IEnumerable<string> pathMatches, HttpRequest request)
        {
            foreach (var pathMatch in pathMatches)
            {
                try
                {
                    if (Regex.IsMatch(request.Path, pathMatch))
                    {
                        return true;
                    }
                }
                catch (ArgumentException ex)
                {
                    _logger.LogError($"Error while matching with pattern {pathMatch}: {ex.Message}.", ex);
                }
            }

            return false;
        }
    }
}
