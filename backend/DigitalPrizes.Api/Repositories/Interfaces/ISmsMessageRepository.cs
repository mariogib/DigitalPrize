using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;

namespace DigitalPrizes.Api.Repositories.Interfaces;

/// <summary>
/// Repository interface for SmsMessage entities.
/// </summary>
public interface ISmsMessageRepository : IRepository<SmsMessage, long>
{
    /// <summary>
    /// Gets SMS messages with paging.
    /// </summary>
    Task<PagedResponse<SmsMessage>> GetPagedAsync(
        FilterParameters parameters,
        string? status = null,
        string? messageType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending SMS messages for sending.
    /// </summary>
    Task<IReadOnlyList<SmsMessage>> GetPendingAsync(
        int maxCount = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets failed SMS messages for retry.
    /// </summary>
    Task<IReadOnlyList<SmsMessage>> GetFailedForRetryAsync(
        int maxRetries = 3,
        int maxCount = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates SMS message status.
    /// </summary>
    Task UpdateStatusAsync(
        long smsMessageId,
        string status,
        string? providerReference = null,
        string? providerResponse = null,
        string? failureReason = null,
        CancellationToken cancellationToken = default);
}
