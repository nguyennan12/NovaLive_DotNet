using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaLive.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductSoftDeleteIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_skus_shop_id_sku_code",
                schema: "public",
                table: "skus");

            migrationBuilder.CreateIndex(
                name: "ix_skus_shop_id_sku_code",
                schema: "public",
                table: "skus",
                columns: new[] { "shop_id", "sku_code" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_skus_shop_id_sku_code",
                schema: "public",
                table: "skus");

            migrationBuilder.CreateIndex(
                name: "ix_skus_shop_id_sku_code",
                schema: "public",
                table: "skus",
                columns: new[] { "shop_id", "sku_code" },
                unique: true);
        }
    }
}
