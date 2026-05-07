using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishapp.Web.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "proposals");

            migrationBuilder.CreateTable(
                name: "gift_proposals",
                schema: "proposals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_type = table.Column<int>(type: "integer", nullable: false),
                    catalog_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    wishlist_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    custom_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    custom_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    custom_image_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    hint_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    recipient_comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_viewed_by_recipient = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reacted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gift_proposals", x => x.id);
                    table.CheckConstraint("ck_gift_proposals_source_type", "source_type IN (1, 2, 3)");
                    table.CheckConstraint("ck_gift_proposals_status", "status IN (0, 1, 2)");
                });

            migrationBuilder.CreateIndex(
                name: "ix_gift_proposals_recipient_id_is_viewed_by_recipient_created_",
                schema: "proposals",
                table: "gift_proposals",
                columns: new[] { "recipient_id", "is_viewed_by_recipient", "created_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_gift_proposals_sender_id_created_at",
                schema: "proposals",
                table: "gift_proposals",
                columns: new[] { "sender_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gift_proposals",
                schema: "proposals");
        }
    }
}
