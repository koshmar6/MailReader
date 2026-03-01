# OpenTelemetry Configuration

This document describes the OpenTelemetry setup for the MailReader project, including integration with .NET Aspire.

## Overview

OpenTelemetry is configured across all services to collect:
- **Logs**: Structured logging with correlation IDs
- **Traces**: Distributed tracing across service boundaries
- **Metrics**: Performance and operational metrics

## Services with OpenTelemetry

1. **MailReader.Api** - Web API service
2. **MailReader.Worker** - Background worker service
3. **MailReader.Migrations** - Database migration tool

## Configuration Details

### Common Configuration

All services share a common OpenTelemetry configuration:

```csharp
var serviceName = "MailReader.{ServiceName}";
var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName, serviceVersion: serviceVersion)
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName,
            ["host.name"] = Environment.MachineName
        }))
    .WithTracing(tracing => tracing
        .AddEntityFrameworkCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("Wolverine")
        .AddSource("MediatR")
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.AddOtlpExporter();
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});
```

### Service-Specific Instrumentation

#### MailReader.Api
- ASP.NET Core instrumentation (HTTP requests/responses)
- Entity Framework Core instrumentation (database queries)
- Hangfire instrumentation (background jobs)
- HTTP client instrumentation (outgoing requests)
- Wolverine source (message bus)
- MediatR source (command/query handling)

#### MailReader.Worker
- Entity Framework Core instrumentation
- Hangfire instrumentation (job execution)
- HTTP client instrumentation
- Wolverine source (message bus)
- MediatR source
- Hangfire source (job scheduling)

#### MailReader.Migrations
- Entity Framework Core instrumentation
- HTTP client instrumentation
- Custom source for migration operations

## Aspire Integration

### OpenTelemetry Exporter

All services use the OTLP (OpenTelemetry Protocol) exporter to send telemetry data. When running with Aspire, the OTLP endpoint is automatically configured by the Aspire dashboard.

### Aspire Dashboard

When you run the AppHost project, Aspire automatically:
1. Starts an OpenTelemetry collector
2. Configures all services to export telemetry to the collector
3. Displays telemetry in the Aspire dashboard

### Viewing Telemetry

1. Start the AppHost project:
   ```bash
   dotnet run --project MailReader.AppHost
   ```

2. Open the Aspire Dashboard (usually at `http://localhost:18888`)

3. Navigate to the following sections:
   - **Structured Logs**: View all logs from all services
   - **Traces**: View distributed traces across services
   - **Metrics**: View performance metrics

## Environment Variables

OpenTelemetry can be configured using environment variables:

### OTLP Endpoint
```bash
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
```

### Service Name Override
```bash
OTEL_SERVICE_NAME=MailReader.Api
```

### Resource Attributes
```bash
OTEL_RESOURCE_ATTRIBUTES=deployment.environment=production,service.version=1.0.0
```

## Instrumentation Libraries

The following OpenTelemetry instrumentation libraries are used:

| Library | Version | Purpose |
|---------|---------|---------|
| OpenTelemetry | 1.11.1 | Core OpenTelemetry SDK |
| OpenTelemetry.Extensions.Hosting | 1.11.1 | Hosting integration |
| OpenTelemetry.Instrumentation.AspNetCore | 1.11.1 | ASP.NET Core instrumentation |
| OpenTelemetry.Instrumentation.EntityFrameworkCore | 1.0.0-beta.12 | EF Core instrumentation |
| OpenTelemetry.Instrumentation.Hangfire | 1.0.0-beta.1 | Hangfire instrumentation |
| OpenTelemetry.Instrumentation.Http | 1.11.1 | HTTP client instrumentation |
| OpenTelemetry.Instrumentation.Process | 0.5.0-beta.2 | Process metrics |
| OpenTelemetry.Instrumentation.Runtime | 1.9.0 | .NET runtime metrics |
| Aspire.OpenTelemetry.Exporter.OpenTelemetryProtocol | 13.1.2 | Aspire OTLP exporter |
| Aspire.OpenTelemetry.Exporter.Console | 13.1.2 | Console exporter for debugging |

## Custom Instrumentation

### Adding Custom Spans

```csharp
using System.Diagnostics;

// Create a custom activity
using var activity = DiagnosticSource.StartActivity("CustomOperation");
activity?.SetTag("operation.name", "MyOperation");
activity?.SetTag("operation.type", "Custom");

// Your code here

activity?.SetStatus(ActivityStatusCode.Ok);
```

### Adding Custom Metrics

```csharp
using System.Diagnostics.Metrics;

var meter = new Meter("MailReader.Custom");
var counter = meter.CreateCounter<int>("custom.operations", "operations", "Number of custom operations");

counter.Add(1, new KeyValuePair<string, object?>("operation.type", "Custom"));
```

### Adding Custom Logs

```csharp
using Microsoft.Extensions.Logging;

logger.LogInformation("Processing mailbox {MailboxId}", mailboxId);
logger.LogWarning("Failed to connect to IMAP server {Server}", server);
logger.LogError(ex, "Error processing message {MessageId}", messageId);
```

## Troubleshooting

### No Telemetry Data

1. Check that the OTLP endpoint is accessible
2. Verify environment variables are set correctly
3. Check service logs for OpenTelemetry errors
4. Ensure the Aspire dashboard is running

### Missing Traces

1. Verify that instrumentation is enabled for the relevant library
2. Check that the service name is correctly configured
3. Ensure that the OTLP exporter is properly configured

### High Memory Usage

1. Reduce sampling rate (configure in OTLP exporter)
2. Adjust batch size and export interval
3. Filter out unnecessary instrumentation

## Best Practices

1. **Use Structured Logging**: Include context in log messages using structured parameters
2. **Add Custom Attributes**: Add relevant business context to spans and metrics
3. **Set Appropriate Span Names**: Use descriptive names for custom spans
4. **Handle Errors Properly**: Set appropriate status codes and error messages
5. **Use Semantic Conventions**: Follow OpenTelemetry semantic conventions for attribute names

## Additional Resources

- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/instrumentation/net/)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [OpenTelemetry Semantic Conventions](https://opentelemetry.io/docs/reference/specification/)
