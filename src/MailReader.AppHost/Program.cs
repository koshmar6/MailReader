using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// ── PostgreSQL ─────────────────────────────────────────────────
var postgres = builder.AddPostgres("postgres")
    .WithHostPort(54994) // чтобы всегда одинаково работало создание миграций
    .WithPgAdmin() // pgAdmin UI локально
    .WithDataVolume(); // персистентный volume

var database = postgres.AddDatabase("mailreader");

// ── Kafka ──────────────────────────────────────────────────────
var kafka = builder.AddKafka("kafka")
    .WithKafkaUI() // Kafka UI на localhost
    .WithDataVolume("mailreader-kafka");

// -- Migrations
var migrations = builder.AddProject<MailReader_Migrations>("migrations")
    .WithReference(database);

// ── API ────────────────────────────────────────────────────────
var api = builder.AddProject<MailReader_Api>("api")
    .WithReference(database)
    .WithReference(kafka)
    .WaitFor(database)
    .WaitFor(kafka)
    .WaitForCompletion(migrations);

// ── Worker ─────────────────────────────────────────────────────
// Можно запустить несколько инстансов — имитируем k8s replicas
builder.AddProject<MailReader_Worker>("worker-1")
    .WithReference(database)
    .WithReference(kafka)
    .WaitFor(database)
    .WaitFor(kafka)
    .WaitForCompletion(migrations);

builder.AddProject<MailReader_Worker>("worker-2")
    .WithReference(database)
    .WithReference(kafka)
    .WaitFor(database)
    .WaitFor(kafka)
    .WaitForCompletion(migrations);

await builder.Build().RunAsync();