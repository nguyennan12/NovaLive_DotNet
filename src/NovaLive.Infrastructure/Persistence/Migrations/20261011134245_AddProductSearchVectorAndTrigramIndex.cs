using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace NovaLive.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSearchVectorAndTrigramIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "search_vector",
                schema: "public",
                table: "spus",
                type: "tsvector",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_spus_name_trgm",
                schema: "public",
                table: "spus",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "idx_spus_search_vector",
                schema: "public",
                table: "spus",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_ts_config WHERE cfgname = 'vietnamese') THEN
        CREATE TEXT SEARCH CONFIGURATION vietnamese (COPY = simple);
    END IF;
END $$;

CREATE OR REPLACE FUNCTION spus_search_vector_trigger() RETURNS trigger AS $$
BEGIN
    new.search_vector := to_tsvector('vietnamese', coalesce(new.name, '') || ' ' || coalesce(new.description, '') || ' ' || coalesce(new.brand, ''));
    RETURN new;
END
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_spus_search_vector_update ON spus;
CREATE TRIGGER trg_spus_search_vector_update
BEFORE INSERT OR UPDATE OF name, description, brand ON spus
FOR EACH ROW EXECUTE FUNCTION spus_search_vector_trigger();

UPDATE spus
SET search_vector = to_tsvector('vietnamese', coalesce(name, '') || ' ' || coalesce(description, '') || ' ' || coalesce(brand, ''))
WHERE search_vector IS NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TRIGGER IF EXISTS trg_spus_search_vector_update ON spus;
DROP FUNCTION IF EXISTS spus_search_vector_trigger();
");

            migrationBuilder.DropIndex(
                name: "idx_spus_name_trgm",
                schema: "public",
                table: "spus");

            migrationBuilder.DropIndex(
                name: "idx_spus_search_vector",
                schema: "public",
                table: "spus");

            migrationBuilder.DropColumn(
                name: "search_vector",
                schema: "public",
                table: "spus");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
        }
    }
}
