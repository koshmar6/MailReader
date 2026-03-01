using Hangfire;
using Hangfire.PostgreSql;
using MailReader.Application;
using MailReader.Application.Abstractions;
using MailReader.Application.Jobs;
using MailReader.Application.Messages.Events;
using MailReader.Infrastructure;
using MailReader.Infrastructure.Jobs;
using Wolverine;
using Wolverine.Kafka;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// ── Infrastructure (EF Core, Repositories, MailKit, etc.) ──────
builder.Services.AddInfrastructure(builder.Configuration);

// ── MediatR ────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly));

// ── OpenTelemetry ──────────────────────────────────────────────
var serviceName = "MailReader.Worker";
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
        .AddSource("Hangfire")
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

// ── Hangfire ───────────────────────────────────────────────────
builder.Services.AddScoped<IJobScheduler, HangfireJobScheduler>();
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("mailreader"))));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = builder.Configuration.GetValue("Hangfire:WorkerCount", 10);
    options.Queues = ["default"];
});

// ── Wolverine + Kafka ──────────────────────────────────────────
builder.UseWolverine(opts =>
{
    var kafkaBootstrapServers = builder.Configuration.GetConnectionString("kafka")!;

    opts.UseKafka(kafkaBootstrapServers);

    // Публикуем MailMessageReceivedEvent в топик mail-messages
    opts.PublishMessage<MailMessageReceivedEvent>()
        .ToKafkaTopic("mail-messages");
});

var app = builder.Build();

// ── Регистрируем рекуррентную джобу ───────────────────────────
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<ScheduleMailboxJobsJob>(
        "schedule-mailbox-jobs",
        job => job.ExecuteAsync(CancellationToken.None),
        "*/5 * * * *"); // каждые 5 минут
}

await app.RunAsync();