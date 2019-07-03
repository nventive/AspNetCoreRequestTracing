using Microsoft.Extensions.Logging;

namespace AspNetCoreRequestTracing.Tests
{
    public class TestLoggerProvider : ILoggerProvider
    {
        private readonly TestLogger _testLogger;

        public TestLoggerProvider(TestLogger testLogger)
        {
            _testLogger = testLogger ?? throw new System.ArgumentNullException(nameof(testLogger));
        }

        public ILogger CreateLogger(string categoryName) => _testLogger;

        public void Dispose()
        {
        }
    }
}
