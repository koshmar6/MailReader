using MailReader.Domain.Entities;
using MailReader.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MailReader.Infrastructure.Persistence.Configurations;

public sealed class MailboxConfiguration : IEntityTypeConfiguration<Mailbox>
{
    public void Configure(EntityTypeBuilder<Mailbox> builder)
    {
        builder.ToTable("mailboxes");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.IsActive).HasColumnName("is_active");
        builder.Property(m => m.LastSeenUid).HasColumnName("last_seen_uid");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        // ImapCredentials как Owned Type (все поля в одной таблице)
        builder.OwnsOne(m => m.Credentials, credentials =>
        {
            credentials.Property(c => c.Host)
                .HasColumnName("imap_host")
                .HasMaxLength(500)
                .IsRequired();

            credentials.Property(c => c.Port)
                .HasColumnName("imap_port")
                .IsRequired();

            credentials.Property(c => c.Username)
                .HasColumnName("imap_username")
                .HasMaxLength(500)
                .IsRequired();

            credentials.Property(c => c.Password)
                .HasColumnName("imap_password")
                .HasMaxLength(1000)
                .IsRequired();

            credentials.Property(c => c.UseSsl)
                .HasColumnName("imap_use_ssl")
                .IsRequired();
        });

        builder.Ignore(m => m.DomainEvents);

        builder.HasIndex(m => m.IsActive).HasDatabaseName("ix_mailboxes_is_active");
    }
}

public sealed class MailMessageConfiguration : IEntityTypeConfiguration<MailMessage>
{
    public void Configure(EntityTypeBuilder<MailMessage> builder)
    {
        builder.ToTable("mail_messages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.MailboxId).HasColumnName("mailbox_id");
        builder.Property(m => m.ImapUid).HasColumnName("imap_uid");
        builder.Property(m => m.MessageId).HasColumnName("message_id").HasMaxLength(1000).IsRequired();
        builder.Property(m => m.Subject).HasColumnName("subject").HasMaxLength(2000);
        builder.Property(m => m.FromAddress).HasColumnName("from_address").HasMaxLength(500);
        builder.Property(m => m.FromName).HasColumnName("from_name").HasMaxLength(500);
        builder.Property(m => m.BodyText).HasColumnName("body_text");
        builder.Property(m => m.BodyHtml).HasColumnName("body_html");
        builder.Property(m => m.SentAt).HasColumnName("sent_at");
        builder.Property(m => m.ReceivedAt).HasColumnName("received_at");

        builder.Ignore(m => m.DomainEvents);

        // Дедупликация: один message_id на ящик
        builder.HasIndex(m => new { m.MailboxId, m.MessageId })
            .IsUnique()
            .HasDatabaseName("ix_mail_messages_mailbox_messageid");

        builder.HasIndex(m => new { m.MailboxId, m.ImapUid })
            .HasDatabaseName("ix_mail_messages_mailbox_uid");

        builder.HasIndex(m => m.ReceivedAt)
            .HasDatabaseName("ix_mail_messages_received_at");
    }
}
