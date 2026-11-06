using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaLive.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261106154500_AddLivestreamCartAttribution")]
public partial class AddLivestreamCartAttribution : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "livestream_session_id",
            schema: "public",
            table: "cart_items",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_cart_items_livestream_session_id",
            schema: "public",
            table: "cart_items",
            column: "livestream_session_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_cart_items_livestream_session_id",
            schema: "public",
            table: "cart_items");

        migrationBuilder.DropColumn(
            name: "livestream_session_id",
            schema: "public",
            table: "cart_items");
    }
}
