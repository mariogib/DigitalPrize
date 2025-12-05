using System.Text.Json.Serialization;

namespace DigitalPrizes.Api.Models.Auth;

/// <summary>
/// Database connection information extracted from JWT token.
/// </summary>
public class DatabaseInfo
{
    /// <summary>
    /// Gets or sets the database provider (e.g., "SqlServer").
    /// </summary>
    [JsonPropertyName("Provider")]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database server name.
    /// </summary>
    [JsonPropertyName("Server")]
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    [JsonPropertyName("Database")]
    public string Database { get; set; } = string.Empty;

    /// <summary>
    /// Builds a SQL Server connection string from the database info.
    /// </summary>
    /// <returns>The connection string.</returns>
    public string ToConnectionString()
    {
        return $"Server={Server};Database={Database};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
    }
}
