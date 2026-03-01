using Hangfire;
using Hangfire.PostgreSql;
using MailReader.Api.Endpoints;
using MailReader.Application;
using MailReader.Infrastructure;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.Kafka;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// ── Infrastructure ─────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── MediatR ────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly));

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