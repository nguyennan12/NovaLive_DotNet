using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaLive.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFlashSaleOrderReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "flash_sale_item_id",
                schema: "public",
                table: "order_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "flash_sale_state",
                schema: "public",
                table: "order_items",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_items_flash_sale_item_id",
                schema: "public",
                table: "order_items",
                column: "flash_sale_item_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_flash_sale_items_quantity",
                schema: "public",
                table: "flash_sale_items",
                sql: "quantity > 0 AND per_user_limit > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_flash_sale_items_stock",
                schema: "public",
                table: "flash_sale_items",
                sql: "reserved_qty >= 0 AND sold_qty >= 0 AND reserved_qty + sold_qty <= quantity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_order_items_flash_sale_item_id",
                schema: "public",
                table: "order_items");

            migrationBuilder.DropCheckConstraint(
                name: "ck_flash_sale_items_quantity",
                schema: "public",
                table: "flash_sale_items");

            migrationBuilder.DropCheckConstraint(
                name: "ck_flash_sale_items_stock",
                schema: "public",
                table: "flash_sale_items");

            migrationBuilder.DropColumn(
                name: "flash_sale_item_id",
                schema: "public",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "flash_sale_state",
                schema: "public",
                table: "order_items");
        }
    }
}
