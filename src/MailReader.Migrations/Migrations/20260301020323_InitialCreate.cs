using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MailReader.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mail_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mailbox_id = table.Column<Guid>(type: "uuid", nullable: false),
                    imap_uid = table.Column<long>(type: "bigint", nullable: false),
                    message_id = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    subject = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    from_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    from_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    body_text = table.Column<string>(type: "text", nullable: false),
                    body_html = table.Column<string>(type: "text", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mail_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mailboxes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    imap_host = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    imap_port = table.Column<int>(type: "integer", nullable: false),
                    imap_username = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    imap_password = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    imap_use_ssl = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_seen_uid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mailboxes", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mail_messages_mailbox_messageid",
                table: "mail_messages",
                columns: new[] { "mailbox_id", "message_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mail_messages_mailbox_uid",
                table: "mail_messages",
                columns: new[] { "mailbox_id", "imap_uid" });

            migrationBuilder.CreateIndex(
                name: "ix_mail_messages_received_at",
                table: "mail_messages",
                column: "received_at");

            migrationBuilder.CreateIndex(
                name: "ix_mailboxes_is_active",
                table: "mailboxes",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mail_messages");

            migrationBuilder.DropTable(
                name: "mailboxes");
        }
    }
}
