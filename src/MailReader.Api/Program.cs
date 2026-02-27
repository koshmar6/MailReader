using MailReader.Api.Endpoints;
using MailReader.Application;
using MailReader.Infrastructure;
using Wolverine;
using Wolverine.Kafka;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// ── Infrastructure ─────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── MediatR ────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly));

// ── Swagger ────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();

// ── Wolverine (только для публикации если нужно из API) ────────
builder.UseWolverine(opts =>
{
    var kafkaBootstrapServers = builder.Configuration.GetConnectionString("kafka")!;

    opts.UseKafka(kafkaBootstrapServers);
});

var app = builder.Build();

// ── Endpoints ──────────────────────────────────────────────────
app.MapMailboxEndpoints();

await app.RunAsync();