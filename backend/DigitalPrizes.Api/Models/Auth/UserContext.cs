namespace DigitalPrizes.Api.Models.Auth;

/// <summary>
/// Represents the current user context extracted from JWT claims.
/// </summary>
public class UserContext
{
    /// <summary>
    /// Gets or sets the user's subject ID (unique identifier).
    /// </summary>
    public string SubjectId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant ID.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public string Organization { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the email is verified.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Gets or sets whether the user is a cross-tenant admin.
    /// </summary>
    public bool IsCrossTenantAdmin { get; set; }

    /// <summary>
    /// Gets or sets the original tenant ID (for cross-tenant admins).
    /// </summary>
    public string? OriginalTenantId { get; set; }

    /// <summary>
    /// Gets or sets the user's roles.
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Gets or sets the database connection information.
    /// </summary>
    public DatabaseInfo? DatabaseInfo { get; set; }

    /// <summary>
    /// Gets or sets the client ID that made the request.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the user has the role.</returns>
    public bool HasRole(string role)
    {
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user is an administrator.
    /// </summary>
    /// <returns>True if the user is an admin.</returns>
    public bool IsAdmin()
    {
        return HasRole("Administrator") || HasRole("System Administrator");
    }
}
