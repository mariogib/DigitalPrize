using System.Globalization;
using System.Text;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Reports;
using DigitalPrizes.Api.Repositories.Interfaces;
using DigitalPrizes.Api.Services.Interfaces;

namespace DigitalPrizes.Api.Services.Implementations;

/// <summary>
/// Service implementation for reporting and analytics.
/// </summary>
public class ReportService : IReportService
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IPrizeAwardRepository _prizeAwardRepository;
    private readonly IPrizeRedemptionRepository _prizeRedemptionRepository;
    private readonly IExternalUserRepository _externalUserRepository;
    private readonly ILogger<ReportService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportService"/> class.
    /// </summary>
    /// <param name="competitionRepository">The competition repository.</param>
    /// <param name="registrationRepository">The registration repository.</param>
    /// <param name="prizeAwardRepository">The prize award repository.</param>
    /// <param name="prizeRedemptionRepository">The prize redemption repository.</param>
    /// <param name="externalUserRepository">The external user repository.</param>
    /// <param name="logger">The logger.</param>
    public ReportService(
        ICompetitionRepository competitionRepository,
        IRegistrationRepository registrationRepository,
        IPrizeAwardRepository prizeAwardRepository,
        IPrizeRedemptionRepository prizeRedemptionRepository,
        IExternalUserRepository externalUserRepository,
        ILogger<ReportService> logger)
    {
        _competitionRepository = competitionRepository;
        _registrationRepository = registrationRepository;
        _prizeAwardRepository = prizeAwardRepository;
        _prizeRedemptionRepository = prizeRedemptionRepository;
        _externalUserRepository = externalUserRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<DashboardSummary> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var sevenDaysAgo = now.AddDays(-7);

        // Get active competitions
        var activeCompetitions = await _competitionRepository.GetActiveCompetitionsAsync(cancellationToken);

        // Get award statistics
        var awardStats = await _prizeAwardRepository.GetStatisticsAsync(cancellationToken: cancellationToken);

        // Get external users count
        var externalUsers = await _externalUserRepository.CountAsync(predicate: null, cancellationToken: cancellationToken);

        // Get recent competitions summary
        var recentCompetitions = activeCompetitions
            .OrderByDescending(c => c.StartDate)
            .Take(5)
            .Select(c => new CompetitionSummary
            {
                CompetitionId = c.CompetitionId,
                Name = c.Name,
                Status = c.IsActive ? "Active" : "Inactive",
                RegistrationCount = c.Registrations.Count,
                AwardedCount = c.PrizeAwards.Count,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
            })
            .ToList();

        // Calculate last 7 days stats (simplified)
        var last7DaysStats = new List<DailyStatistic>();
        for (var i = 6; i >= 0; i--)
        {
            var date = now.Date.AddDays(-i);
            last7DaysStats.Add(new DailyStatistic
            {
                Date = date,
                Registrations = 0, // Would require more complex queries
                Awards = 0,
                Redemptions = 0,
            });
        }

        // Count total registrations across all competitions
        var totalRegistrations = activeCompetitions.Sum(c => c.Registrations.Count);

        return new DashboardSummary
        {
            ActiveCompetitions = activeCompetitions.Count,
            TotalRegistrations = totalRegistrations,
            TotalPrizesAwarded = awardStats.TotalAwarded,
            TotalPrizesRedeemed = awardStats.TotalRedeemed,
            PendingRedemptions = awardStats.PendingRedemption,
            TotalPrizeValueAwarded = awardStats.TotalValueAwarded,
            TotalPrizeValueRedeemed = awardStats.TotalValueRedeemed,
            ExternalUsersCount = externalUsers,
            RecentCompetitions = recentCompetitions,
            Last7DaysStats = last7DaysStats,
        };
    }

    /// <inheritdoc/>
    public async Task<RegistrationStats> GetRegistrationStatsAsync(
        int? competitionId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = todayStart.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        IReadOnlyList<Registration> registrations;

        if (competitionId.HasValue)
        {
            registrations = await _registrationRepository.GetByCompetitionAsync(competitionId.Value, cancellationToken);
        }
        else
        {
            // Get all registrations via paged query (simplified for now)
            var pagedResult = await _registrationRepository.GetPagedAsync(
                new Models.Dtos.Common.FilterParameters { PageSize = 10000 },
                competitionId,
                cancellationToken);
            registrations = pagedResult.Items.ToList();
        }

        // Apply date filters
        if (fromDate.HasValue)
        {
            registrations = registrations.Where(r => r.RegistrationDate >= fromDate.Value).ToList();
        }

        if (toDate.HasValue)
        {
            registrations = registrations.Where(r => r.RegistrationDate <= toDate.Value).ToList();
        }

        var byCompetition = registrations
            .GroupBy(r => r.Competition?.Name ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        var dailyBreakdown = registrations
            .Where(r => r.RegistrationDate >= (fromDate ?? now.AddDays(-30)))
            .GroupBy(r => r.RegistrationDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyStatistic
            {
                Date = g.Key,
                Registrations = g.Count(),
            })
            .ToList();

        return new RegistrationStats
        {
            TotalRegistrations = registrations.Count,
            TodayRegistrations = registrations.Count(r => r.RegistrationDate >= todayStart),
            ThisWeekRegistrations = registrations.Count(r => r.RegistrationDate >= weekStart),
            ThisMonthRegistrations = registrations.Count(r => r.RegistrationDate >= monthStart),
            ByCompetition = byCompetition,
            DailyBreakdown = dailyBreakdown,
        };
    }

    /// <inheritdoc/>
    public async Task<AwardStats> GetAwardStatsAsync(
        int? competitionId = null,
        int? prizePoolId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var stats = await _prizeAwardRepository.GetStatisticsAsync(
            competitionId,
            fromDate,
            toDate,
            cancellationToken);

        // Get awards for breakdown
        var parameters = new AwardsReportParameters
        {
            PageSize = 10000,
            CompetitionId = competitionId,
            PrizePoolId = prizePoolId,
            AwardedFrom = fromDate,
            AwardedTo = toDate,
        };

        var awardsResult = await _prizeAwardRepository.GetPagedAsync(parameters, cancellationToken);
        var awards = awardsResult.Items.ToList();

        var byPrizeType = awards
            .GroupBy(a => a.Prize?.PrizeType?.Name ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        var now = DateTime.UtcNow;
        var dailyBreakdown = awards
            .Where(a => a.AwardedAt >= (fromDate ?? now.AddDays(-30)))
            .GroupBy(a => a.AwardedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyStatistic
            {
                Date = g.Key,
                Awards = g.Count(),
                Redemptions = g.Count(a => a.Status == AwardStatus.Redeemed),
            })
            .ToList();

        return new AwardStats
        {
            TotalAwarded = stats.TotalAwarded,
            TotalRedeemed = stats.TotalRedeemed,
            TotalExpired = stats.TotalExpired,
            PendingRedemption = stats.PendingRedemption,
            TotalValueAwarded = stats.TotalValueAwarded,
            TotalValueRedeemed = stats.TotalValueRedeemed,
            ByPrizeType = byPrizeType,
            DailyBreakdown = dailyBreakdown,
        };
    }

    /// <inheritdoc/>
    public async Task<RedemptionStats> GetRedemptionStatsAsync(
        int? competitionId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;

        // Get awards that have been redeemed
        var parameters = new AwardsReportParameters
        {
            PageSize = 10000,
            CompetitionId = competitionId,
            Status = AwardStatus.Redeemed,
            AwardedFrom = fromDate,
            AwardedTo = toDate,
        };

        var awardsResult = await _prizeAwardRepository.GetPagedAsync(parameters, cancellationToken);
        var redeemedAwards = awardsResult.Items
            .Where(a => a.PrizeRedemption is not null)
            .ToList();

        var byChannel = redeemedAwards
            .GroupBy(a => a.PrizeRedemption?.RedeemedChannel ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        var dailyBreakdown = redeemedAwards
            .Where(a => a.PrizeRedemption is not null && a.PrizeRedemption.RedeemedAt >= (fromDate ?? now.AddDays(-30)))
            .GroupBy(a => a.PrizeRedemption!.RedeemedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyStatistic
            {
                Date = g.Key,
                Redemptions = g.Count(),
            })
            .ToList();

        // Calculate average redemption time (time between award and redemption)
        var redemptionTimes = redeemedAwards
            .Where(a => a.PrizeRedemption is not null)
            .Select(a => (a.PrizeRedemption!.RedeemedAt - a.AwardedAt).TotalHours)
            .ToList();

        var avgRedemptionTime = redemptionTimes.Count > 0 ? (decimal)redemptionTimes.Average() : 0;

        return new RedemptionStats
        {
            TotalRedemptions = redeemedAwards.Count,
            TodayRedemptions = redeemedAwards.Count(a =>
                a.PrizeRedemption is not null && a.PrizeRedemption.RedeemedAt >= todayStart),
            AverageRedemptionTime = avgRedemptionTime,
            ByChannel = byChannel,
            DailyBreakdown = dailyBreakdown,
        };
    }

    /// <inheritdoc/>
    public async Task<byte[]> ExportAwardsReportAsync(
        ExportReportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var parameters = new AwardsReportParameters
        {
            PageSize = 100000, // Large export
            CompetitionId = request.CompetitionId,
            PrizePoolId = request.PrizePoolId,
            AwardedFrom = request.DateFrom,
            AwardedTo = request.DateTo,
        };

        var result = await _prizeAwardRepository.GetPagedAsync(parameters, cancellationToken);
        var awards = result.Items.ToList();

        _logger.LogInformation("Exporting {Count} awards to {Format}", awards.Count, request.Format);

        return string.Equals(request.Format, "CSV", StringComparison.OrdinalIgnoreCase)
            ? GenerateAwardsCsv(awards)
            : GenerateAwardsCsv(awards); // Default to CSV for now
    }

    /// <inheritdoc/>
    public async Task<byte[]> ExportRegistrationsReportAsync(
        ExportReportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var parameters = new Models.Dtos.Common.FilterParameters
        {
            PageSize = 100000, // Large export
        };

        var result = await _registrationRepository.GetPagedAsync(parameters, request.CompetitionId, cancellationToken);
        var registrations = result.Items
            .Where(r => (!request.DateFrom.HasValue || r.RegistrationDate >= request.DateFrom.Value) &&
                       (!request.DateTo.HasValue || r.RegistrationDate <= request.DateTo.Value))
            .ToList();

        _logger.LogInformation("Exporting {Count} registrations to {Format}", registrations.Count, request.Format);

        return string.Equals(request.Format, "CSV", StringComparison.OrdinalIgnoreCase)
            ? GenerateRegistrationsCsv(registrations)
            : GenerateRegistrationsCsv(registrations); // Default to CSV for now
    }

    private static byte[] GenerateAwardsCsv(List<PrizeAward> awards)
    {
        var sb = new StringBuilder();
        sb.AppendLine("PrizeAwardId,CellNumber,PrizeName,PrizeType,CompetitionName,AwardedAt,Status,ExpiryDate,NotificationStatus");

        foreach (var award in awards)
        {
            var line = string.Join(
                ",",
                award.PrizeAwardId.ToString(CultureInfo.InvariantCulture),
                EscapeCsvField(award.CellNumber),
                EscapeCsvField(award.Prize?.Name ?? string.Empty),
                EscapeCsvField(award.Prize?.PrizeType?.Name ?? string.Empty),
                EscapeCsvField(award.Competition?.Name ?? string.Empty),
                award.AwardedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                EscapeCsvField(award.Status),
                award.ExpiryDate?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty,
                EscapeCsvField(award.NotificationStatus ?? string.Empty));
            sb.AppendLine(line);
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static byte[] GenerateRegistrationsCsv(List<Registration> registrations)
    {
        var sb = new StringBuilder();
        sb.AppendLine("RegistrationId,CellNumber,CompetitionName,RegistrationDate,Source,FirstName,LastName,Email");

        foreach (var reg in registrations)
        {
            var line = string.Join(
                ",",
                reg.RegistrationId.ToString(CultureInfo.InvariantCulture),
                EscapeCsvField(reg.CellNumber),
                EscapeCsvField(reg.Competition?.Name ?? string.Empty),
                reg.RegistrationDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                EscapeCsvField(reg.Source ?? string.Empty),
                EscapeCsvField(reg.ExternalUser?.FirstName ?? string.Empty),
                EscapeCsvField(reg.ExternalUser?.LastName ?? string.Empty),
                EscapeCsvField(reg.ExternalUser?.Email ?? string.Empty));
            sb.AppendLine(line);
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
        {
            return string.Empty;
        }

        if (field.Contains(',', StringComparison.Ordinal) ||
            field.Contains('"', StringComparison.Ordinal) ||
            field.Contains('\n', StringComparison.Ordinal))
        {
            return $"\"{field.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return field;
    }
}
