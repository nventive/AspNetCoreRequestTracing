using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace AspNetCoreRequestTracing.Tests
{
    public class TestLoggerMessage
    {
        public LogLevel LogLevel { get; set; }

        public EventId EventId { get; set; }

        public IReadOnlyList<KeyValuePair<string, object>> State { get; set; }

        public IDictionary<string, object> StateDictionary { get => State.ToDictionary(x => x.Key, x => x.Value); }

        public Exception Exception { get; set; }
    }
}
