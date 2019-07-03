using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetCoreRequestTracing
{
    /// <summary>
    /// Helper class that manages <see cref="ILogger"/> events for <see cref="RequestTracingMiddleware"/>.
    /// </summary>
    internal static class RequestTracingMiddlewareLoggerExtensions
    {
        private const string RequestMessageFormat = @"
{RequestMethod} {RequestUri} {RequestProtocol}
{RequestHeaders}

{RequestBody}";

        private const string ResponseMessageFormat = @"
{ResponseProtocol} {ResponseStatusCode}
{ResponseHeaders}

{ResponseBody}";

        private static readonly Action<ILogger, string, string, string, string, string, Exception> _requestTrace =
            LoggerMessage.Define<string, string, string, string, string>(
                LogLevel.Trace,
                new EventId(300, "RequestTrace"),
                RequestMessageFormat);

        private static readonly Action<ILogger, string, int, string, string, Exception> _responseTrace =
            LoggerMessage.Define<string, int, string, string>(
                LogLevel.Trace,
                new EventId(310, "ResponseTrace"),
                ResponseMessageFormat);

        private static readonly Action<ILogger, string, int, string, string, Exception> _responseWarning =
            LoggerMessage.Define<string, int, string, string>(
                LogLevel.Warning,
                new EventId(311, "ResponseWarning"),
                ResponseMessageFormat);

        private static readonly Action<ILogger, string, int, string, string, Exception> _responseError =
            LoggerMessage.Define<string, int, string, string>(
                LogLevel.Error,
                new EventId(312, "ResponseError"),
                ResponseMessageFormat);

        /// <summary>
        /// Traces the full <paramref name="request"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="request">The <see cref="HttpRequest"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task RequestTrace(this ILogger logger, HttpRequest request)
        {
            if (!logger.IsEnabled(LogLevel.Trace))
            {
                return;
            }

            if (request.Body != null && !request.Body.CanSeek)
            {
                return;
            }

            request.Body?.Seek(0, SeekOrigin.Begin);

            _requestTrace(
                logger,
                request.Method,
                $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}",
                request.Protocol,
                request.Headers.AllHeadersAsString(),
                request.Body == null ? string.Empty : await new StreamReader(request.Body).ReadToEndAsync(),
                null);

            request.Body?.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Traces the full <paramref name="response"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="response">The <see cref="HttpResponse"/>.</param>
        /// <param name="responseBody">The response body. This <see cref="Stream"/> must be seakable to be able to rewind and read.</param>
        /// <param name="protocol">The HTTP Protocol.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task ResponseTrace(this ILogger logger, HttpResponse response, Stream responseBody, string protocol)
        {
            if (!logger.IsEnabled(LogLevel.Trace))
            {
                return;
            }

            responseBody.Seek(0, SeekOrigin.Begin);

            _responseTrace(
                logger,
                protocol,
                response.StatusCode,
                response.Headers.AllHeadersAsString(),
                await new StreamReader(responseBody).ReadToEndAsync(),
                null);
        }

        /// <summary>
        /// Traces the full <paramref name="response"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="response">The <see cref="HttpResponse"/>.</param>
        /// <param name="responseBody">The response body. This <see cref="Stream"/> must be seakable to be able to rewind and read.</param>
        /// <param name="protocol">The HTTP Protocol.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task ResponseWarning(this ILogger logger, HttpResponse response, Stream responseBody, string protocol)
        {
            if (!logger.IsEnabled(LogLevel.Warning))
            {
                return;
            }

            responseBody.Seek(0, SeekOrigin.Begin);

            _responseWarning(
                logger,
                protocol,
                response.StatusCode,
                response.Headers.AllHeadersAsString(),
                await new StreamReader(responseBody).ReadToEndAsync(),
                null);
        }

        /// <summary>
        /// Traces the full <paramref name="response"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="response">The <see cref="HttpResponse"/>.</param>
        /// <param name="responseBody">The response body. This <see cref="Stream"/> must be seakable to be able to rewind and read.</param>
        /// <param name="protocol">The HTTP Protocol.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task ResponseError(this ILogger logger, HttpResponse response, Stream responseBody, string protocol)
        {
            if (!logger.IsEnabled(LogLevel.Error))
            {
                return;
            }

            responseBody.Seek(0, SeekOrigin.Begin);

            _responseError(
                logger,
                protocol,
                response.StatusCode,
                response.Headers.AllHeadersAsString(),
                await new StreamReader(responseBody).ReadToEndAsync(),
                null);
        }
    }
}
