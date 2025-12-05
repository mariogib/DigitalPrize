namespace DigitalPrizes.Api.Models.Dtos.Reports;

/// <summary>
/// Dashboard summary statistics.
/// </summary>
public record DashboardSummary
{
    public int ActiveCompetitions { get; init; }
    public int TotalRegistrations { get; init; }
    public int TotalPrizesAwarded { get; init; }
    public int TotalPrizesRedeemed { get; init; }
    public int PendingRedemptions { get; init; }
    public decimal TotalPrizeValueAwarded { get; init; }
    public decimal TotalPrizeValueRedeemed { get; init; }
    public int ExternalUsersCount { get; init; }
    public List<CompetitionSummary> RecentCompetitions { get; init; } = new();
    public List<DailyStatistic> Last7DaysStats { get; init; } = new();
}

/// <summary>
/// Competition summary for dashboard.
/// </summary>
public record CompetitionSummary
{
    public int CompetitionId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int RegistrationCount { get; init; }
    public int AwardedCount { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

/// <summary>
/// Daily statistics for charts.
/// </summary>
public record DailyStatistic
{
    public DateTime Date { get; init; }
    public int Registrations { get; init; }
    public int Awards { get; init; }
    public int Redemptions { get; init; }
}

/// <summary>
/// Prize awards report parameters.
/// </summary>
public record AwardsReportParameters : Dtos.Common.FilterParameters
{
    public int? CompetitionId { get; init; }
    public int? PrizePoolId { get; init; }
    public int? PrizeTypeId { get; init; }
    public string? Status { get; init; }
    public DateTime? AwardedFrom { get; init; }
    public DateTime? AwardedTo { get; init; }
}

/// <summary>
/// Redemptions report parameters.
/// </summary>
public record RedemptionsReportParameters : Dtos.Common.FilterParameters
{
    public int? CompetitionId { get; init; }
    public int? PrizePoolId { get; init; }
    public string? RedemptionChannel { get; init; }
    public DateTime? RedeemedFrom { get; init; }
    public DateTime? RedeemedTo { get; init; }
}

/// <summary>
/// Export report request.
/// </summary>
public record ExportReportRequest
{
    public string ReportType { get; init; } = string.Empty; // Awards, Redemptions, Registrations, Users
    public string Format { get; init; } = "CSV"; // CSV, Excel
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public int? CompetitionId { get; init; }
    public int? PrizePoolId { get; init; }
    public List<string>? Columns { get; init; }
}

/// <summary>
/// Audit log report entry.
/// </summary>
public record AuditLogEntry
{
    public long AuditLogId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string? SubjectName { get; init; }
    public DateTime Timestamp { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// Audit log filter parameters.
/// </summary>
public record AuditLogParameters : Dtos.Common.FilterParameters
{
    public string? EntityType { get; init; }
    public string? Action { get; init; }
    public string? SubjectId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}
