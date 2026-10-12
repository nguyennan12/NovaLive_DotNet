using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaLive.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RelaxPermissionActionConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE permission_action_constraint RECORD;
                BEGIN
                    FOR permission_action_constraint IN
                        SELECT constraint_data.conname
                        FROM pg_constraint AS constraint_data
                        INNER JOIN pg_class AS table_data
                            ON table_data.oid = constraint_data.conrelid
                        INNER JOIN pg_namespace AS schema_data
                            ON schema_data.oid = table_data.relnamespace
                        WHERE schema_data.nspname = 'public'
                            AND table_data.relname = 'permissions'
                            AND constraint_data.contype = 'c'
                            AND pg_get_constraintdef(constraint_data.oid) ILIKE '%action%'
                    LOOP
                        EXECUTE format(
                            'ALTER TABLE public.permissions DROP CONSTRAINT %I',
                            permission_action_constraint.conname);
                    END LOOP;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE public.permissions
                ADD CONSTRAINT ck_permissions_action
                CHECK (action IN ('Create', 'Read', 'Update', 'Delete', 'Approve', 'Export', 'Override', 'Suspend'))
                NOT VALID;
                """);
        }
    }
}
