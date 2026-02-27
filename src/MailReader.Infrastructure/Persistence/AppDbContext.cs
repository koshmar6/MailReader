using MailReader.Domain.Entities;
using MailReader.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MailReader.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Mailbox> Mailboxes => Set<Mailbox>();
    public DbSet<MailMessage> MailMessages => Set<MailMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return base.SaveChangesAsync(ct);
    }
}
