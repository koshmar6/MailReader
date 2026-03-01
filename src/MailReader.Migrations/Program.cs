using MailReader.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Получаем строку подключения из переменных окружения или appsettings
var connectionString = builder.Configuration.GetConnectionString("mailreader");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("Database connection string is not configured.");
    Environment.Exit(1);
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsql => npgsql.MigrationsAssembly("MailReader.Migrations")));

var app = builder.Build();

Console.WriteLine("Applying database migrations...");

try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        Console.WriteLine($"Found {pendingMigrations.Count()} pending migration(s). Applying...");
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("Migrations applied successfully.");
    }
    else
    {
        Console.WriteLine("No pending migrations found. Database is up to date.");
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error applying migrations: {ex.Message}");
    Environment.Exit(1);
}

Console.WriteLine("Migration process completed.");