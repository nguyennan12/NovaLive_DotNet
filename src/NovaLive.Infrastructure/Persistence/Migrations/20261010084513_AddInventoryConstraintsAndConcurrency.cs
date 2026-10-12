using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaLive.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryConstraintsAndConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ref_type",
                schema: "public",
                table: "inventory_histories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "note",
                schema: "public",
                table: "inventory_histories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "operation_id",
                schema: "public",
                table: "inventory_histories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "version",
                schema: "public",
                table: "inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_histories_inventory_id",
                schema: "public",
                table: "inventory_histories",
                column: "inventory_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_histories_operation_id",
                schema: "public",
                table: "inventory_histories",
                column: "operation_id",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "chk_inventory_min_stock_non_negative",
                schema: "public",
                table: "inventories",
                sql: "min_stock >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "chk_inventory_on_hand_gte_reserved",
                schema: "public",
                table: "inventories",
                sql: "qty_on_hand >= reserved_qty");

            migrationBuilder.AddCheckConstraint(
                name: "chk_inventory_qty_on_hand_non_negative",
                schema: "public",
                table: "inventories",
                sql: "qty_on_hand >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "chk_inventory_reserved_qty_non_negative",
                schema: "public",
                table: "inventories",
                sql: "reserved_qty >= 0");

            migrationBuilder.AddForeignKey(
                name: "fk_inventories_skus_sku_id",
                schema: "public",
                table: "inventories",
                column: "sku_id",
                principalSchema: "public",
                principalTable: "skus",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_histories_inventories_inventory_id",
                schema: "public",
                table: "inventory_histories",
                column: "inventory_id",
                principalSchema: "public",
                principalTable: "inventories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_histories_skus_sku_id",
                schema: "public",
                table: "inventory_histories",
                column: "sku_id",
                principalSchema: "public",
                principalTable: "skus",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_inventories_skus_sku_id",
                schema: "public",
                table: "inventories");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_histories_inventories_inventory_id",
                schema: "public",
                table: "inventory_histories");

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_histories_skus_sku_id",
                schema: "public",
                table: "inventory_histories");

            migrationBuilder.DropIndex(
                name: "ix_inventory_histories_inventory_id",
                schema: "public",
                table: "inventory_histories");

            migrationBuilder.DropIndex(
                name: "ix_inventory_histories_operation_id",
                schema: "public",
                table: "inventory_histories");

            migrationBuilder.DropCheckConstraint(
                name: "chk_inventory_min_stock_non_negative",
                schema: "public",
                table: "inventories");

            migrationBuilder.DropCheckConstraint(
                name: "chk_inventory_on_hand_gte_reserved",
                schema: "public",
                table: "inventories");

            migrationBuilder.DropCheckConstraint(
                name: "chk_inventory_qty_on_hand_non_negative",
                schema: "public",
                table: "inventories");

            migrationBuilder.DropCheckConstraint(
                name: "chk_inventory_reserved_qty_non_negative",
                schema: "public",
                table: "inventories");

            migrationBuilder.DropColumn(
                name: "operation_id",
                schema: "public",
                table: "inventory_histories");

            migrationBuilder.DropColumn(
                name: "version",
                schema: "public",
                table: "inventories");

            migrationBuilder.AlterColumn<string>(
                name: "ref_type",
                schema: "public",
                table: "inventory_histories",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "note",
                schema: "public",
                table: "inventory_histories",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
