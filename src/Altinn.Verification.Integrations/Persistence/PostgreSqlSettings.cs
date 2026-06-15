namespace Altinn.Verification.Integrations.Persistence;

/// <summary>
/// Settings for PostgreSQL database connection.
/// </summary>
public class PostgreSqlSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the database connection is enabled.
    /// </summary>
    public bool EnableDBConnection { get; set; } = true;

    /// <summary>
    /// Gets or sets the connection string for the admin user.
    /// </summary>
    public string AdminConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for the admin user.
    /// </summary>
    public string VerificationDbAdminPwd { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connection string for the app user.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for the app user.
    /// </summary>
    public string VerificationDbPwd { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to include parameter values in logging/tracing.
    /// </summary>
    public bool LogParameters { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to run the database in debug mode.
    /// </summary>
    public bool EnableDebug { get; set; } = false;
}
