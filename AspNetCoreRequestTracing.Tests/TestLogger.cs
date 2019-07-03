using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AspNetCoreRequestTracing.Tests
{
    public class TestLogger : ILogger
    {
        public IList<TestLoggerMessage> Messages { get; } = new List<TestLoggerMessage>();

        public IDisposable BeginScope<TState>(TState state) => new EmptyDisposable();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Messages.Add(new TestLoggerMessage
            {
                LogLevel = logLevel,
                EventId = eventId,
                State = (IReadOnlyList<KeyValuePair<string, object>>)state,
                Exception = exception,
            });
        }

        private class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
