using DigitalPrizes.Api.Data;
using DigitalPrizes.Api.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrizes.Api.Services.Auth;

/// <summary>
/// Interface for creating database contexts with dynamic connection strings.
/// </summary>
public interface IDatabaseContextFactory
{
    /// <summary>
    /// Creates a database context using the connection string from the current user's token.
    /// </summary>
    /// <returns>The database context.</returns>
    ApplicationDbContext CreateContext();
}

/// <summary>
/// Factory for creating database contexts with connection strings from JWT tokens.
/// </summary>
public class DatabaseContextFactory : IDatabaseContextFactory
{
    private readonly IUserContextService _userContextService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseContextFactory> _logger;

    public DatabaseContextFactory(
        IUserContextService userContextService,
        IConfiguration configuration,
        ILogger<DatabaseContextFactory> logger)
    {
        _userContextService = userContextService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public ApplicationDbContext CreateContext()
    {
        var connectionString = _userContextService.GetConnectionString();

        if (string.IsNullOrEmpty(connectionString))
        {
            // Fall back to default connection string from configuration
            connectionString = _configuration.GetConnectionString("DefaultConnection");
            _logger.LogDebug("Using default connection string from configuration");
        }
        else
        {
            _logger.LogDebug("Using connection string from JWT token");
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "No database connection string available. Ensure the user is authenticated with a valid db_info claim or configure a default connection string.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
