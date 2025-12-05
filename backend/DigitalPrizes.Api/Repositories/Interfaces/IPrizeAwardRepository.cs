using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Reports;

namespace DigitalPrizes.Api.Repositories.Interfaces;

/// <summary>
/// Repository interface for PrizeAward entities.
/// </summary>
public interface IPrizeAwardRepository : IRepository<PrizeAward, long>
{
    /// <summary>
    /// Gets prize awards with paging and filtering.
    /// </summary>
    Task<PagedResponse<PrizeAward>> GetPagedAsync(
        AwardsReportParameters parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a prize award with all related data.
    /// </summary>
    Task<PrizeAward?> GetWithRelationsAsync(long prizeAwardId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets prize awards for a cell number.
    /// </summary>
    Task<IReadOnlyList<PrizeAward>> GetByCellNumberAsync(
        string cellNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets redeemable awards for a cell number.
    /// </summary>
    Task<IReadOnlyList<PrizeAward>> GetRedeemableAsync(
        string cellNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets prize awards for a competition.
    /// </summary>
    Task<IReadOnlyList<PrizeAward>> GetByCompetitionAsync(
        int competitionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets award statistics for reporting.
    /// </summary>
    Task<AwardStatistics> GetStatisticsAsync(
        int? competitionId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Award statistics for reporting.
/// </summary>
public record AwardStatistics
{
    public int TotalAwarded { get; init; }
    public int TotalRedeemed { get; init; }
    public int TotalExpired { get; init; }
    public int TotalCancelled { get; init; }
    public int PendingRedemption { get; init; }
    public decimal TotalValueAwarded { get; init; }
    public decimal TotalValueRedeemed { get; init; }
}
