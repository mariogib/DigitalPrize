using DigitalPrizes.Api.Models.Dtos.Awards;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Reports;

namespace DigitalPrizes.Api.Services.Interfaces;

/// <summary>
/// Service interface for prize awarding.
/// </summary>
public interface IPrizeAwardService
{
    /// <summary>
    /// Gets prize awards with paging and filtering.
    /// </summary>
    /// <param name="parameters">Paging and filtering parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of awards.</returns>
    Task<PagedResponse<PrizeAwardResponse>> GetAwardsAsync(
        AwardsReportParameters parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a prize award by ID.
    /// </summary>
    /// <param name="prizeAwardId">Prize award identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Award details if found.</returns>
    Task<PrizeAwardDetailResponse?> GetAwardAsync(long prizeAwardId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets awards for a cell number.
    /// </summary>
    /// <param name="cellNumber">Cell number to search.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of awards.</returns>
    Task<IReadOnlyList<PrizeAwardResponse>> GetAwardsByCellNumberAsync(
        string cellNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Awards a prize to a user.
    /// </summary>
    /// <param name="request">Award prize request.</param>
    /// <param name="subjectId">Subject performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created award if successful.</returns>
    Task<PrizeAwardResponse?> AwardPrizeAsync(
        AwardPrizeRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk awards prizes to multiple users.
    /// </summary>
    /// <param name="request">Bulk award request.</param>
    /// <param name="subjectId">Subject performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Bulk award response.</returns>
    Task<BulkAwardResponse> BulkAwardPrizesAsync(
        BulkAwardPrizesRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a prize award.
    /// </summary>
    /// <param name="prizeAwardId">Prize award identifier.</param>
    /// <param name="request">Cancel award request.</param>
    /// <param name="subjectId">Subject performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if cancelled.</returns>
    Task<bool> CancelAwardAsync(
        long prizeAwardId,
        CancelAwardRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resends notification for an award.
    /// </summary>
    /// <param name="prizeAwardId">Prize award identifier.</param>
    /// <param name="notificationChannel">Notification channel to use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if sent.</returns>
    Task<bool> ResendNotificationAsync(
        long prizeAwardId,
        string? notificationChannel = null,
        CancellationToken cancellationToken = default);
}
