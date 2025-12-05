using DigitalPrizes.Api.Models.Dtos.Reports;

namespace DigitalPrizes.Api.Services.Interfaces;

/// <summary>
/// Service interface for reporting and analytics.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Gets dashboard summary statistics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dashboard summary.</returns>
    Task<DashboardSummary> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets registration statistics.
    /// </summary>
    /// <param name="competitionId">Optional competition filter.</param>
    /// <param name="fromDate">Optional start date filter.</param>
    /// <param name="toDate">Optional end date filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Registration statistics.</returns>
    Task<RegistrationStats> GetRegistrationStatsAsync(
        int? competitionId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets prize award statistics.
    /// </summary>
    /// <param name="competitionId">Optional competition filter.</param>
    /// <param name="prizePoolId">Optional prize pool filter.</param>
    /// <param name="fromDate">Optional start date filter.</param>
    /// <param name="toDate">Optional end date filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Award statistics.</returns>
    Task<AwardStats> GetAwardStatsAsync(
        int? competitionId = null,
        int? prizePoolId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets redemption statistics.
    /// </summary>
    /// <param name="competitionId">Optional competition filter.</param>
    /// <param name="fromDate">Optional start date filter.</param>
    /// <param name="toDate">Optional end date filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redemption statistics.</returns>
    Task<RedemptionStats> GetRedemptionStatsAsync(
        int? competitionId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports awards report.
    /// </summary>
    /// <param name="request">Export request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Report bytes.</returns>
    Task<byte[]> ExportAwardsReportAsync(
        ExportReportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports registrations report.
    /// </summary>
    /// <param name="request">Export request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Report bytes.</returns>
    Task<byte[]> ExportRegistrationsReportAsync(
        ExportReportRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Registration statistics.
/// </summary>
public record RegistrationStats
{
    public int TotalRegistrations { get; init; }
    public int TodayRegistrations { get; init; }
    public int ThisWeekRegistrations { get; init; }
    public int ThisMonthRegistrations { get; init; }
    public Dictionary<string, int> ByCompetition { get; init; } = new();
    public List<DailyStatistic> DailyBreakdown { get; init; } = new();
}

/// <summary>
/// Award statistics.
/// </summary>
public record AwardStats
{
    public int TotalAwarded { get; init; }
    public int TotalRedeemed { get; init; }
    public int TotalExpired { get; init; }
    public int PendingRedemption { get; init; }
    public decimal TotalValueAwarded { get; init; }
    public decimal TotalValueRedeemed { get; init; }
    public Dictionary<string, int> ByPrizeType { get; init; } = new();
    public List<DailyStatistic> DailyBreakdown { get; init; } = new();
}

/// <summary>
/// Redemption statistics.
/// </summary>
public record RedemptionStats
{
    public int TotalRedemptions { get; init; }
    public int TodayRedemptions { get; init; }
    public decimal AverageRedemptionTime { get; init; } // in hours
    public Dictionary<string, int> ByChannel { get; init; } = new();
    public List<DailyStatistic> DailyBreakdown { get; init; } = new();
}
