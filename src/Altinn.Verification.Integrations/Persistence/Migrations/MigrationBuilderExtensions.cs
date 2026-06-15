using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore.Migrations;

namespace Altinn.Verification.Integrations.Persistence.Migrations;

/// <summary>
/// Extension methods for MigrationBuilder to help with database grants.
/// </summary>
[ExcludeFromCodeCoverage]
public static class MigrationBuilderExtensions
{
    private const string RuntimeDbUser = "platform_verification";

    /// <summary>
    /// Adds a grant statement for schema permissions for the runtime database user to the migration.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder.</param>
    /// <param name="schema">The schema name.</param>
    /// <param name="permissions">The schema permissions to grant (default: USAGE).</param>
    public static void GrantSchemaPermissions(
        this MigrationBuilder migrationBuilder,
        string schema,
        string permissions = "USAGE")
    {
        var grantSql = $"GRANT {permissions} ON SCHEMA {QuoteIdentifier(schema)} TO {RuntimeDbUser};";
        migrationBuilder.Sql(grantSql);
    }

    /// <summary>
    /// Adds a grant statement for the runtime database user to the migration.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder.</param>
    /// <param name="schema">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="permissions">The permissions to grant (default: SELECT, INSERT, UPDATE, DELETE).</param>
    public static void GrantTablePermissions(
        this MigrationBuilder migrationBuilder,
        string schema,
        string tableName,
        string permissions = "SELECT, INSERT, UPDATE, DELETE")
    {
        var grantSql = $"GRANT {permissions} ON TABLE {QuoteIdentifier(schema)}.{QuoteIdentifier(tableName)} TO {RuntimeDbUser};";
        migrationBuilder.Sql(grantSql);
    }

    /// <summary>
    /// Revokes permissions from the runtime database user for a table.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder.</param>
    /// <param name="schema">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="permissions">The permissions to revoke (default: SELECT, INSERT, UPDATE, DELETE).</param>
    public static void RevokeTablePermissions(
        this MigrationBuilder migrationBuilder,
        string schema,
        string tableName,
        string permissions = "SELECT, INSERT, UPDATE, DELETE")
    {
        var revokeSql = $"REVOKE {permissions} ON TABLE {schema}.{tableName} FROM {RuntimeDbUser};";
        migrationBuilder.Sql(revokeSql, suppressTransaction: true);
    }

    private static string QuoteIdentifier(string identifier) =>
        $"\"{identifier.Replace("\"", "\"\"")}\"";
}
