using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaLive.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RestoreCartItemUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // InitialCreate already created this column. A later snapshot omitted it
            // without dropping the physical column. Support both database histories.
            migrationBuilder.Sql("""
                ALTER TABLE public.cart_items ADD COLUMN IF NOT EXISTS updated_at timestamptz;
                UPDATE public.cart_items SET updated_at = added_at WHERE updated_at IS NULL;
                ALTER TABLE public.cart_items ALTER COLUMN updated_at SET NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Preserve the historical column and its data when reverting metadata.
        }
    }
}
