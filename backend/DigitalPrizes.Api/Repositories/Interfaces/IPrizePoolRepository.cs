using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;

namespace DigitalPrizes.Api.Repositories.Interfaces;

/// <summary>
/// Repository interface for PrizePool entities.
/// </summary>
public interface IPrizePoolRepository : IRepository<PrizePool, int>
{
    /// <summary>
    /// Gets prize pools with paging and filtering.
    /// </summary>
    Task<PagedResponse<PrizePool>> GetPagedAsync(
        FilterParameters parameters,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a prize pool with its prizes.
    /// </summary>
    Task<PrizePool?> GetWithPrizesAsync(int prizePoolId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active prize pools with available prizes.
    /// </summary>
    Task<IReadOnlyList<PrizePool>> GetActiveWithAvailablePrizesAsync(CancellationToken cancellationToken = default);
}
