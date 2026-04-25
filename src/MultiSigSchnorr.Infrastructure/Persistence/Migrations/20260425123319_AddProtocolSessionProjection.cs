using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiSigSchnorr.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProtocolSessionProjection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "protocol_sessions",
                columns: table => new
                {
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    epoch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    epoch_number = table.Column<int>(type: "integer", nullable: false),
                    session_status = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    protection_mode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    message_digest_hex = table.Column<string>(type: "text", nullable: false),
                    aggregate_public_key_hex = table.Column<string>(type: "text", nullable: false),
                    aggregate_nonce_point_hex = table.Column<string>(type: "text", nullable: true),
                    challenge_hex = table.Column<string>(type: "text", nullable: true),
                    aggregate_signature_nonce_point_hex = table.Column<string>(type: "text", nullable: true),
                    aggregate_signature_scalar_hex = table.Column<string>(type: "text", nullable: true),
                    all_commitments_published = table.Column<bool>(type: "boolean", nullable: false),
                    all_nonces_revealed = table.Column<bool>(type: "boolean", nullable: false),
                    all_partial_signatures_submitted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_protocol_sessions", x => x.session_id);
                });

            migrationBuilder.CreateTable(
                name: "protocol_session_participants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    has_commitment = table.Column<bool>(type: "boolean", nullable: false),
                    has_reveal = table.Column<bool>(type: "boolean", nullable: false),
                    has_partial_signature = table.Column<bool>(type: "boolean", nullable: false),
                    public_key_hex = table.Column<string>(type: "text", nullable: false),
                    aggregation_coefficient_hex = table.Column<string>(type: "text", nullable: false),
                    commitment_hex = table.Column<string>(type: "text", nullable: true),
                    public_nonce_point_hex = table.Column<string>(type: "text", nullable: true),
                    partial_signature_hex = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_protocol_session_participants", x => x.id);
                    table.ForeignKey(
                        name: "FK_protocol_session_participants_protocol_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "protocol_sessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_protocol_session_participants_session_id_participant_id",
                table: "protocol_session_participants",
                columns: new[] { "session_id", "participant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_protocol_sessions_created_utc",
                table: "protocol_sessions",
                column: "created_utc");

            migrationBuilder.CreateIndex(
                name: "IX_protocol_sessions_epoch_id",
                table: "protocol_sessions",
                column: "epoch_id");

            migrationBuilder.CreateIndex(
                name: "IX_protocol_sessions_protection_mode",
                table: "protocol_sessions",
                column: "protection_mode");

            migrationBuilder.CreateIndex(
                name: "IX_protocol_sessions_session_status",
                table: "protocol_sessions",
                column: "session_status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "protocol_session_participants");

            migrationBuilder.DropTable(
                name: "protocol_sessions");
        }
    }
}
