using System;
using Altinn.Verification.Integrations.Persistence.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Altinn.Verification.Integrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "address_verifications");

            migrationBuilder.CreateTable(
                name: "verification_codes",
                schema: "address_verifications",
                columns: table => new
                {
                    verification_code_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    failed_attempts = table.Column<int>(type: "integer", nullable: false),
                    expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verification_code_hash = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    address_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("verification_code_id_pkey", x => x.verification_code_id);
                });

            migrationBuilder.CreateTable(
                name: "verified_addresses",
                schema: "address_verifications",
                columns: table => new
                {
                    verified_address_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    address_type = table.Column<string>(type: "text", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_addresses", x => x.verified_address_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_verification_codes_user_id_address_address_type",
                schema: "address_verifications",
                table: "verification_codes",
                columns: new[] { "user_id", "address", "address_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_verified_addresses_user_id_address_address_type",
                schema: "address_verifications",
                table: "verified_addresses",
                columns: new[] { "user_id", "address", "address_type" },
                unique: true);

            // Grant permissions to the runtime database user
            migrationBuilder.GrantSchemaPermissions("address_verifications");
            migrationBuilder.GrantTablePermissions("address_verifications", "verification_codes");
            migrationBuilder.GrantTablePermissions("address_verifications", "verified_addresses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RevokeTablePermissions("address_verifications", "verification_codes");
            migrationBuilder.RevokeTablePermissions("address_verifications", "verified_addresses");

            migrationBuilder.DropTable(
                name: "verification_codes",
                schema: "address_verifications");

            migrationBuilder.DropTable(
                name: "verified_addresses",
                schema: "address_verifications");
        }
    }
}
