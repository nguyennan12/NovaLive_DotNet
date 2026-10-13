using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaLive.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShopAddressDefaultIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The existing cart_items.updated_at column is intentionally retained: its model drift
            // predates T11. The snapshot describes the current model, but this migration only adds shop indexes.
            migrationBuilder.CreateIndex(
                name: "ix_shop_addresses_primary",
                schema: "public",
                table: "shop_addresses",
                column: "shop_id",
                unique: true,
                filter: "is_primary = TRUE");

            migrationBuilder.CreateIndex(
                name: "ix_shop_addresses_return",
                schema: "public",
                table: "shop_addresses",
                column: "shop_id",
                unique: true,
                filter: "is_return = TRUE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_shop_addresses_primary",
                schema: "public",
                table: "shop_addresses");

            migrationBuilder.DropIndex(
                name: "ix_shop_addresses_return",
                schema: "public",
                table: "shop_addresses");
        }
    }
}
