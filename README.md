# AspNetCoreRequestTracing

Full Request/Response Tracing for ASP.NET Core

This projects provides an ASP.NET Core middleware to allow complete tracing of
requests and responses (including the body) going in/out of an ASP.NET Core app.

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)
[![Build Status](https://uno-platform.visualstudio.com/Backend%20Projects/_apis/build/status/nventive.AspNetCoreRequestTracing?branchName=master)](https://uno-platform.visualstudio.com/Backend%20Projects/_build/latest?definitionId=59&branchName=master)

## Getting Started

Install the package:

```
Install-Package AspNetCoreRequestTracing
```

Then in the `Startup` class adds the configuration and the request middleware:

```csharp

using AspNetCoreRequestTracing;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        // ...

        // Configure the RequestTracingMiddlewareOptions. This is one way of doing it.
        services.Configure<RequestTracingMiddlewareOptions>(options =>
        {
            // List of regular expressions used to match the incoming requests path
            // for which tracing is enabled.
            // If the request path is not matched, no tracing will happen and the middleware
            // is a no-op.
            options.EnableFor = new[] { "/info", "/api/.*" };
        });
        
        // Alternatively, options can be loaded from a configuration section.
        services.Configure<RequestTracingMiddlewareOptions>(
          _configuration.GetSection(nameof(RequestTracingMiddlewareOptions)));
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        // Make sure this is sufficiently early in the request pipeline.
        app.UseRequestTracing();
        // ...
        app.UseMvc();
    }
}

```

## Features

### Request selection

Only requests that match a path configured in `RequestTracingMiddlewareOptions.EnabledFor` will be traced;
This is done because enabling tracing may have an impact on performance, as the request and the response needs buffering.

In order to trace _all requests_, just use a catch-all regular expression: `".*"`.

### Logger settings

The logger category is `AspNetCoreRequestTracing.RequestTracingMiddleware`.
The `LogLevel` used for request is always `LogLevel.Trace`.

For the responses, it depends on the `StatusCode`:
- Any status code below 400 is logged using `LogLevel.Trace`
- Any status code above 400 and below 500 is logged using `LogLevel.Warning`
- Any status code above 500 is logged using `LogLevel.Error`

Event ids used for the various messages:

| Event id | Event Name      | Description                        |
|----------|-----------------|------------------------------------|
| 300      | RequestTrace    | Incoming request                   |
| 310      | ResponseTrace   | Successful response                |
| 311      | ResponseWarning | Response with a client error (4xx) |
| 312      | ResponseError   | Response with a server error (5xx) |

To enable the logger, please consult the documentation regarding [Logging in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.2#configuration).

One way of configuring it is through the `appsettings.json` file:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "AspNetCoreRequestTracing.RequestTracingMiddleware": "Trace"
    }
  }
}
```

### Usage with Application Insights

When adding [Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core), by default all `Warning` and `Error` trace are captured,
which means that:
- no request information will be captured in Application Insights
- only responses with either a client error (4xx) or a server error (5xx) will be captured

To enable the capture for the requests and successful responses, [please configure the
`ApplicationInsightsLoggerProvider` to keep the `Trace` level](https://docs.microsoft.com/en-us/azure/azure-monitor/app/ilogger). This can be achieved through
the `appsettings.json` file as well:

```json
{
  "Logging": {
    "ApplicationInsights": {
      "LogLevel": {
        "AspNetCoreRequestTracing.RequestTracingMiddleware": "Trace"
      }
    },
    "LogLevel": {
      "Default": "Warning",
      "AspNetCoreRequestTracing.RequestTracingMiddleware": "Trace"
    }
  }
}
```

### Performance impacts

Adding the middleware with no matching requests should have a negligible impact
on the performance of the app.
However, when a request path is matched, then several things need to happen to 
be able to fully trace the request and the response:

- `EnableRewind()` is called on the `Request`; this enables buffering for the requests
- a secondary buffer for the `Response` is created to capture the writes in parallel to
sending them back to the host server; this incurs additional processing and memory overhead; secondary buffered are pooled to minimize [LOH](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/large-object-heap) allocations.

## Changelog

Please consult the [CHANGELOG](CHANGELOG.md) for more information about version
history.

## License

This project is licensed under the Apache 2.0 license - see the
[LICENSE](LICENSE) file for details.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on the process for
contributing to this project.

Be mindful of our [Code of Conduct](CODE_OF_CONDUCT.md).

## Acknowledgments

In order to mitigate the effect of allocating large buffers continuously, this
package uses pooled memory streams provided by the `Microsoft.IO.RecyclableMemoryStream` package.
