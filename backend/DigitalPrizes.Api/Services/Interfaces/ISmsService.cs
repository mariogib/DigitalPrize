namespace DigitalPrizes.Api.Services.Interfaces;

/// <summary>
/// Service interface for SMS messaging.
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Sends an SMS message.
    /// </summary>
    /// <param name="cellNumber">Cell number to send to.</param>
    /// <param name="message">Message content.</param>
    /// <param name="messageType">Type of message.</param>
    /// <param name="relatedEntityType">Related entity type.</param>
    /// <param name="relatedEntityId">Related entity identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>SMS result.</returns>
    Task<SmsResult> SendSmsAsync(
        string cellNumber,
        string message,
        string messageType,
        string? relatedEntityType = null,
        long? relatedEntityId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an OTP SMS.
    /// </summary>
    /// <param name="cellNumber">Cell number to send to.</param>
    /// <param name="otpCode">OTP code to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>SMS result.</returns>
    Task<SmsResult> SendOtpSmsAsync(
        string cellNumber,
        string otpCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a prize notification SMS.
    /// </summary>
    /// <param name="cellNumber">Cell number to send to.</param>
    /// <param name="prizeName">Name of the prize.</param>
    /// <param name="redemptionLink">Redemption link.</param>
    /// <param name="expiryDate">Prize expiry date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>SMS result.</returns>
    Task<SmsResult> SendPrizeNotificationAsync(
        string cellNumber,
        string prizeName,
        string? redemptionLink = null,
        DateTime? expiryDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a redemption confirmation SMS.
    /// </summary>
    /// <param name="cellNumber">Cell number to send to.</param>
    /// <param name="prizeName">Name of the prize.</param>
    /// <param name="redemptionCode">Redemption code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>SMS result.</returns>
    Task<SmsResult> SendRedemptionConfirmationAsync(
        string cellNumber,
        string prizeName,
        string? redemptionCode = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// SMS sending result.
/// </summary>
public record SmsResult
{
    /// <summary>Gets a value indicating whether the SMS was sent successfully.</summary>
    public bool Success { get; init; }

    /// <summary>Gets the SMS message ID.</summary>
    public long? SmsMessageId { get; init; }

    /// <summary>Gets the provider reference.</summary>
    public string? ProviderReference { get; init; }

    /// <summary>Gets the error message if failed.</summary>
    public string? ErrorMessage { get; init; }
}
