using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreRequestTracing
{
    /// <summary>
    /// <see cref="IHeaderDictionary"/> extension methods.
    /// </summary>
    internal static class HeaderDictionaryExtensions
    {
        /// <summary>
        /// Returns all headers as a standard HTTP headers string.
        /// </summary>
        /// <param name="headers">The <see cref="IHeaderDictionary"/>.</param>
        /// <returns>The headers as HTTP headers string.</returns>
        public static string AllHeadersAsString(this IHeaderDictionary headers)
            => string.Join(Environment.NewLine, headers.Select(x => $"{x.Key}: {string.Join(" ", x.Value)}"));
    }
}
