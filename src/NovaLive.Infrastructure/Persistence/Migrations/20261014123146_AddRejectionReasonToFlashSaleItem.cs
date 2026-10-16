using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaLive.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionReasonToFlashSaleItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                schema: "public",
                table: "flash_sale_items",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rejection_reason",
                schema: "public",
                table: "flash_sale_items");
        }
    }
}
