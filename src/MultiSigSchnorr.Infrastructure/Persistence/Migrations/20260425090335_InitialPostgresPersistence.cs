using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiSigSchnorr.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgresPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_log_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    action_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "epochs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activated_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_epochs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "participants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    public_key_hex = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_participants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "epoch_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    epoch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    added_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    removed_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_epoch_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_epoch_members_epochs_epoch_id",
                        column: x => x.epoch_id,
                        principalTable: "epochs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_epoch_members_participants_participant_id",
                        column: x => x.participant_id,
                        principalTable: "participants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_entries_action_type",
                table: "audit_log_entries",
                column: "action_type");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_entries_created_utc",
                table: "audit_log_entries",
                column: "created_utc");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_entries_entity_id",
                table: "audit_log_entries",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_entries_entity_type",
                table: "audit_log_entries",
                column: "entity_type");

            migrationBuilder.CreateIndex(
                name: "IX_epoch_members_epoch_id_participant_id",
                table: "epoch_members",
                columns: new[] { "epoch_id", "participant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_epoch_members_participant_id",
                table: "epoch_members",
                column: "participant_id");

            migrationBuilder.CreateIndex(
                name: "IX_epochs_number",
                table: "epochs",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_participants_display_name",
                table: "participants",
                column: "display_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log_entries");

            migrationBuilder.DropTable(
                name: "epoch_members");

            migrationBuilder.DropTable(
                name: "epochs");

            migrationBuilder.DropTable(
                name: "participants");
        }
    }
}
