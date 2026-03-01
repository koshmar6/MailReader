using MailReader.Application.Abstractions;
using MailReader.Domain.Interfaces;
using MailReader.Infrastructure.Imap;
using MailReader.Infrastructure.Messaging;
using MailReader.Infrastructure.Persistence;
using MailReader.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MailReader.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("mailreader"),
                npgsql => npgsql.MigrationsAssembly("MailReader.Migrations")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IMailboxRepository, MailboxRepository>();
        services.AddScoped<IMailMessageRepository, MailMessageRepository>();

        services.AddScoped<IImapService, MailKitImapService>();
        services.AddScoped<IMessageBus, WolverineMessageBus>();

        return services;
    }
}