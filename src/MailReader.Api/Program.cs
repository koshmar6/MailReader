using Hangfire;
using Hangfire.PostgreSql;
using MailReader.Api.Endpoints;
using MailReader.Application;
using MailReader.Infrastructure;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.Kafka;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// ── Infrastructure ─────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── MediatR ────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly));

// ── OpenTelemetry ──────────────────────────────────────────────
var serviceName = "MailReader.Api";
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
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("Wolverine")
        .AddSource("MediatR")
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
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

// ── OpenAPI ────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── Wolverine (только для публикации если нужно из API) ────────
builder.UseWolverine(opts =>
{
    var kafkaBootstrapServers = builder.Configuration.GetConnectionString("kafka")!;

    opts.UseKafka(kafkaBootstrapServers);
});

// ── Hangfire ───────────────────────────────────────────────────
builder.Services.AddHangfire(config => config
    .UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("mailreader"))));

var app = builder.Build();

// ── Hangfire Dashboard ─────────────────────────────────────────
app.UseHangfireDashboard();

// ── Scalar UI ──────────────────────────────────────────────────
app.MapOpenApi();
app.MapScalarApiReference();

// ── Endpoints ──────────────────────────────────────────────────
app.MapMailboxEndpoints();

await app.RunAsync();