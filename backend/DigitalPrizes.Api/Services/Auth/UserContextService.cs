using System.Security.Claims;
using System.Text.Json;
using DigitalPrizes.Api.Models.Auth;

namespace DigitalPrizes.Api.Services.Auth;

/// <summary>
/// Interface for accessing the current user context.
/// </summary>
public interface IUserContextService
{
    /// <summary>
    /// Gets the current user context from the HTTP context.
    /// </summary>
    /// <returns>The user context, or null if not authenticated.</returns>
    UserContext? GetUserContext();

    /// <summary>
    /// Gets the database connection string from the current user's token.
    /// </summary>
    /// <returns>The connection string, or null if not available.</returns>
    string? GetConnectionString();

    /// <summary>
    /// Gets the current user's subject ID.
    /// </summary>
    /// <returns>The subject ID, or null if not authenticated.</returns>
    string? GetSubjectId();

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    /// <returns>True if authenticated.</returns>
    bool IsAuthenticated();
}

/// <summary>
/// Service for accessing the current user context from JWT claims.
/// </summary>
public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserContextService> _logger;
    private UserContext? _cachedContext;

    public UserContextService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserContextService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <inheritdoc />
    public UserContext? GetUserContext()
    {
        if (_cachedContext != null)
        {
            return _cachedContext;
        }

        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        try
        {
            _cachedContext = BuildUserContext(user);
            return _cachedContext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build user context from claims");
            return null;
        }
    }

    /// <inheritdoc />
    public string? GetConnectionString()
    {
        var context = GetUserContext();
        return context?.DatabaseInfo?.ToConnectionString();
    }

    /// <inheritdoc />
    public string? GetSubjectId()
    {
        return GetUserContext()?.SubjectId;
    }

    /// <inheritdoc />
    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }

    private UserContext BuildUserContext(ClaimsPrincipal user)
    {
        var context = new UserContext
        {
            SubjectId = GetClaimValue(user, "sub") ?? string.Empty,
            TenantId = GetClaimValue(user, "tid") ?? string.Empty,
            Organization = GetClaimValue(user, "org") ?? string.Empty,
            Name = GetClaimValue(user, "name") ?? string.Empty,
            Email = GetClaimValue(user, "email") ?? string.Empty,
            EmailVerified = GetClaimValue(user, "email_verified")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true,
            IsCrossTenantAdmin = GetClaimValue(user, "cross_tenant_admin")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true,
            OriginalTenantId = GetClaimValue(user, "original_tenant_id"),
            ClientId = GetClaimValue(user, "client_id") ?? GetClaimValue(user, "azp") ?? string.Empty,
        };

        // Parse roles - can be a single claim or multiple claims
        var roleClaims = user.FindAll("role").Select(c => c.Value).ToList();
        if (roleClaims.Count == 0)
        {
            var singleRole = GetClaimValue(user, ClaimTypes.Role);
            if (!string.IsNullOrEmpty(singleRole))
            {
                roleClaims.Add(singleRole);
            }
        }

        context.Roles = roleClaims;

        // Parse database info from db_info claim
        var dbInfoJson = GetClaimValue(user, "db_info");
        if (!string.IsNullOrEmpty(dbInfoJson))
        {
            try
            {
                context.DatabaseInfo = JsonSerializer.Deserialize<DatabaseInfo>(dbInfoJson);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse db_info claim: {DbInfo}", dbInfoJson);
            }
        }

        return context;
    }

    private static string? GetClaimValue(ClaimsPrincipal user, string claimType)
    {
        return user.FindFirst(claimType)?.Value;
    }
}
