using DigitalPrizes.Api.Models.Dtos.Awards;
using DigitalPrizes.Api.Models.Dtos.Common;

namespace DigitalPrizes.Api.Services.Interfaces;

/// <summary>
/// Service interface for prize redemption.
/// </summary>
public interface IRedemptionService
{
    /// <summary>
    /// Gets redemptions with paging and filtering.
    /// </summary>
    /// <param name="parameters">Filter parameters.</param>
    /// <param name="competitionId">Optional competition filter.</param>
    /// <param name="prizePoolId">Optional prize pool filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of redemptions.</returns>
    Task<PagedResponse<PrizeRedemptionResponse>> GetRedemptionsAsync(
        FilterParameters parameters,
        int? competitionId = null,
        int? prizePoolId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates a redemption process for a user.
    /// </summary>
    /// <param name="request">Redemption initiation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Initiation response.</returns>
    Task<InitiateRedemptionResponse> InitiateRedemptionAsync(
        InitiateRedemptionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a prize redemption.
    /// </summary>
    /// <param name="request">Redemption completion request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completion response.</returns>
    Task<CompleteRedemptionResponse> CompleteRedemptionAsync(
        CompleteRedemptionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets redeemable prizes for a cell number.
    /// </summary>
    /// <param name="cellNumber">Cell number to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of redeemable prizes.</returns>
    Task<IReadOnlyList<RedeemablePrizeResponse>> GetRedeemablePrizesAsync(
        string cellNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets redemption details for an award.
    /// </summary>
    /// <param name="prizeAwardId">Prize award identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redemption details if found.</returns>
    Task<PrizeRedemptionResponse?> GetRedemptionAsync(
        long prizeAwardId,
        CancellationToken cancellationToken = default);
}
