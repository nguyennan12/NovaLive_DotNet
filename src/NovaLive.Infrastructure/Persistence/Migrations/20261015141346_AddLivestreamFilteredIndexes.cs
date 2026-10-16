using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaLive.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLivestreamFilteredIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_livestream_sessions_shop_id",
                schema: "public",
                table: "livestream_sessions",
                column: "shop_id",
                unique: true,
                filter: "status = 'Live'");

            migrationBuilder.CreateIndex(
                name: "ix_livestream_products_session_id",
                schema: "public",
                table: "livestream_products",
                column: "session_id",
                unique: true,
                filter: "is_pinned = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_livestream_sessions_shop_id",
                schema: "public",
                table: "livestream_sessions");

            migrationBuilder.DropIndex(
                name: "ix_livestream_products_session_id",
                schema: "public",
                table: "livestream_products");
        }
    }
}
