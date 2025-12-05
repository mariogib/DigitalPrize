using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;

namespace DigitalPrizes.Api.Repositories.Interfaces;

/// <summary>
/// Repository interface for PrizeRedemption entities.
/// </summary>
public interface IPrizeRedemptionRepository : IRepository<PrizeRedemption, long>
{
    /// <summary>
    /// Gets redemptions with paging and filtering.
    /// </summary>
    Task<PagedResponse<PrizeRedemption>> GetPagedAsync(
        FilterParameters parameters,
        int? competitionId = null,
        int? prizePoolId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a redemption by prize award ID.
    /// </summary>
    Task<PrizeRedemption?> GetByPrizeAwardIdAsync(
        long prizeAwardId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a redemption with all related data.
    /// </summary>
    Task<PrizeRedemption?> GetWithRelationsAsync(
        long prizeRedemptionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets redemptions for a cell number.
    /// </summary>
    Task<IReadOnlyList<PrizeRedemption>> GetByCellNumberAsync(
        string cellNumber,
        CancellationToken cancellationToken = default);
}
