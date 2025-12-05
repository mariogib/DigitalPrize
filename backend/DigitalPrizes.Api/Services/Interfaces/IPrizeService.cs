using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Prizes;

namespace DigitalPrizes.Api.Services.Interfaces;

/// <summary>
/// Service interface for prize management.
/// </summary>
public interface IPrizeService
{
    /// <summary>
    /// Gets prizes with paging and filtering.
    /// </summary>
    /// <param name="parameters">Paging and filtering parameters.</param>
    /// <param name="prizePoolId">Optional prize pool filter.</param>
    /// <param name="prizeTypeId">Optional prize type filter.</param>
    /// <param name="isActive">Optional active status filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of prizes.</returns>
    Task<PagedResponse<PrizeSummaryResponse>> GetPrizesAsync(
        FilterParameters parameters,
        int? prizePoolId = null,
        int? prizeTypeId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a prize by ID.
    /// </summary>
    /// <param name="prizeId">The prize identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Prize details if found.</returns>
    Task<PrizeDetailResponse?> GetPrizeAsync(long prizeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new prize.
    /// </summary>
    /// <param name="request">Create prize request.</param>
    /// <param name="subjectId">Subject performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created prize.</returns>
    Task<PrizeSummaryResponse> CreatePrizeAsync(
        CreatePrizeRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk creates prizes.
    /// </summary>
    /// <param name="request">Bulk create request.</param>
    /// <param name="subjectId">Subject performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of created prizes.</returns>
    Task<IReadOnlyList<PrizeSummaryResponse>> BulkCreatePrizesAsync(
        BulkCreatePrizesRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a prize.
    /// </summary>
    /// <param name="prizeId">The prize identifier.</param>
    /// <param name="request">Update prize request.</param>
    /// <param name="subjectId">Subject performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated prize if found.</returns>
    Task<PrizeSummaryResponse?> UpdatePrizeAsync(
        long prizeId,
        UpdatePrizeRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a prize.
    /// </summary>
    /// <param name="prizeId">The prize identifier.</param>
    /// <param name="subjectId">Subject performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted.</returns>
    Task<bool> DeletePrizeAsync(long prizeId, string? subjectId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets prize types.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of prize types.</returns>
    Task<IReadOnlyList<PrizeTypeResponse>> GetPrizeTypesAsync(CancellationToken cancellationToken = default);
}
