using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;

namespace DigitalPrizes.Api.Repositories.Interfaces;

/// <summary>
/// Repository interface for Prize entities.
/// </summary>
public interface IPrizeRepository : IRepository<Prize, long>
{
    /// <summary>
    /// Gets prizes with paging and filtering.
    /// </summary>
    /// <param name="parameters">Paging and filtering parameters.</param>
    /// <param name="prizePoolId">Optional prize pool filter.</param>
    /// <param name="prizeTypeId">Optional prize type filter.</param>
    /// <param name="isActive">Optional active status filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged response of prizes.</returns>
    Task<PagedResponse<Prize>> GetPagedAsync(
        FilterParameters parameters,
        int? prizePoolId = null,
        int? prizeTypeId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets prizes with paging (returns tuple for backward compatibility).
    /// </summary>
    /// <param name="pageNumber">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="prizePoolId">Optional prize pool filter.</param>
    /// <param name="prizeTypeId">Optional prize type filter.</param>
    /// <param name="isActive">Optional active status filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of prizes and total count.</returns>
    Task<(IReadOnlyList<Prize> Prizes, int TotalCount)> GetPagedTupleAsync(
        int pageNumber,
        int pageSize,
        int? prizePoolId = null,
        int? prizeTypeId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a prize with all related data.
    /// </summary>
    /// <param name="prizeId">The prize ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Prize with relations if found.</returns>
    Task<Prize?> GetWithRelationsAsync(long prizeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a prize by ID with full details.
    /// </summary>
    /// <param name="prizeId">The prize ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Prize with details if found.</returns>
    Task<Prize?> GetByIdWithDetailsAsync(long prizeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets prizes in a prize pool.
    /// </summary>
    /// <param name="prizePoolId">The prize pool ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of prizes.</returns>
    Task<IReadOnlyList<Prize>> GetByPrizePoolAsync(
        int prizePoolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available prizes for awarding.
    /// </summary>
    /// <param name="prizePoolId">Optional prize pool filter.</param>
    /// <param name="prizeTypeId">Optional prize type filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of available prizes.</returns>
    Task<IReadOnlyList<Prize>> GetAvailableForAwardAsync(
        int? prizePoolId = null,
        int? prizeTypeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next available prize from a pool for awarding.
    /// </summary>
    /// <param name="prizePoolId">The prize pool ID.</param>
    /// <param name="prizeTypeId">Optional prize type filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Next available prize if any.</returns>
    Task<Prize?> GetNextAvailableAsync(
        int prizePoolId,
        int? prizeTypeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Decrements the available quantity of a prize.
    /// </summary>
    /// <param name="prizeId">The prize ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if decremented successfully.</returns>
    Task<bool> DecrementAvailableQuantityAsync(long prizeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple prizes.
    /// </summary>
    /// <param name="prizes">The prizes to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    Task AddRangeAsync(IEnumerable<Prize> prizes, CancellationToken cancellationToken = default);
}
