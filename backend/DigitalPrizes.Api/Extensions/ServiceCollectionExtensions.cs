using DigitalPrizes.Api.Data;
using DigitalPrizes.Api.Repositories.Implementations;
using DigitalPrizes.Api.Repositories.Interfaces;
using DigitalPrizes.Api.Services.Auth;
using DigitalPrizes.Api.Services.Implementations;
using DigitalPrizes.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrizes.Api.Extensions;

/// <summary>
/// Extension methods for service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Auth Services
        services.AddScoped<IUserContextService, UserContextService>();
        services.AddScoped<IDatabaseContextFactory, DatabaseContextFactory>();

        // Database Context - configured per request based on JWT token
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            // Try to get connection string from user context (JWT token)
            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var userContext = httpContextAccessor?.HttpContext?.User;

            string? connectionString = null;

            // If authenticated, try to extract db_info from token
            if (userContext?.Identity?.IsAuthenticated == true)
            {
                var dbInfoClaim = userContext.FindFirst("db_info")?.Value;
                if (!string.IsNullOrEmpty(dbInfoClaim))
                {
                    try
                    {
                        var dbInfo = System.Text.Json.JsonSerializer.Deserialize<Models.Auth.DatabaseInfo>(dbInfoClaim);
                        if (dbInfo != null)
                        {
                            connectionString = dbInfo.ToConnectionString();
                        }
                    }
                    catch
                    {
                        // Fall through to default
                    }
                }
            }

            // Fall back to default connection string
            connectionString ??= configuration.GetConnectionString("DefaultConnection");

            if (!string.IsNullOrEmpty(connectionString))
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            }
        });

        // Repositories
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ICompetitionRepository, CompetitionRepository>();
        services.AddScoped<IExternalUserRepository, ExternalUserRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IPrizeAwardRepository, PrizeAwardRepository>();
        services.AddScoped<IPrizePoolRepository, PrizePoolRepository>();
        services.AddScoped<IPrizeRedemptionRepository, PrizeRedemptionRepository>();
        services.AddScoped<IPrizeRepository, PrizeRepository>();
        services.AddScoped<IPrizeTypeRepository, PrizeTypeRepository>();
        services.AddScoped<IRegistrationRepository, RegistrationRepository>();
        services.AddScoped<ISmsMessageRepository, SmsMessageRepository>();

        // Services
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ICompetitionService, CompetitionService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IPrizeAwardService, PrizeAwardService>();
        services.AddScoped<IPrizePoolService, PrizePoolService>();
        services.AddScoped<IPrizeService, PrizeService>();
        services.AddScoped<IRedemptionService, RedemptionService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ISmsService, SmsService>();

        return services;
    }
}
