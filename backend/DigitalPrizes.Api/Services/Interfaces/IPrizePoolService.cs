using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Prizes;

namespace DigitalPrizes.Api.Services.Interfaces;

/// <summary>
/// Service interface for prize pool management.
/// </summary>
public interface IPrizePoolService
{
    /// <summary>
    /// Gets prize pools with paging and filtering.
    /// </summary>
    /// <param name="parameters">Paging and filtering parameters.</param>
    /// <param name="competitionId">Optional competition filter (kept for backwards compatibility but unused).</param>
    /// <param name="isActive">Optional active status filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of prize pools.</returns>
    Task<PagedResponse<PrizePoolSummaryResponse>> GetPrizePoolsAsync(
        FilterParameters parameters,
        int? competitionId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a prize pool by ID.
    /// </summary>
    /// <param name="prizePoolId">The prize pool identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Prize pool details if found.</returns>
    Task<PrizePoolDetailResponse?> GetPrizePoolAsync(int prizePoolId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new prize pool.
    /// </summary>
    /// <param name="request">Create prize pool request.</param>
    /// <param name="subjectId">Subject performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created prize pool.</returns>
    Task<PrizePoolSummaryResponse> CreatePrizePoolAsync(
        CreatePrizePoolRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a prize pool.
    /// </summary>
    /// <param name="prizePoolId">The prize pool identifier.</param>
    /// <param name="request">Update prize pool request.</param>
    /// <param name="subjectId">Subject performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated prize pool if found.</returns>
    Task<PrizePoolSummaryResponse?> UpdatePrizePoolAsync(
        int prizePoolId,
        UpdatePrizePoolRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a prize pool.
    /// </summary>
    /// <param name="prizePoolId">The prize pool identifier.</param>
    /// <param name="subjectId">Subject performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted.</returns>
    Task<bool> DeletePrizePoolAsync(int prizePoolId, string? subjectId = null, CancellationToken cancellationToken = default);
}
