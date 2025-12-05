using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Repositories.Interfaces;
using DigitalPrizes.Api.Services.Interfaces;

namespace DigitalPrizes.Api.Services.Implementations;

/// <summary>
/// Service implementation for SMS messaging.
/// </summary>
public class SmsService : ISmsService
{
    private readonly ISmsMessageRepository _smsMessageRepository;
    private readonly ILogger<SmsService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmsService"/> class.
    /// </summary>
    /// <param name="smsMessageRepository">The SMS message repository.</param>
    /// <param name="logger">The logger.</param>
    public SmsService(ISmsMessageRepository smsMessageRepository, ILogger<SmsService> logger)
    {
        _smsMessageRepository = smsMessageRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SmsResult> SendSmsAsync(
        string cellNumber,
        string message,
        string messageType,
        string? relatedEntityType = null,
        long? relatedEntityId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cellNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var smsMessage = new SmsMessage
        {
            CellNumber = cellNumber,
            Message = message,
            MessageType = messageType,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
        };

        try
        {
            // In a real implementation, this would call an SMS gateway API
            // For now, we simulate a successful send
            var providerReference = await SendToSmsProviderAsync(cellNumber, message, cancellationToken);

            smsMessage.Status = "Sent";
            smsMessage.SentAt = DateTime.UtcNow;
            smsMessage.ProviderReference = providerReference;

            await _smsMessageRepository.AddAsync(smsMessage, cancellationToken);

            _logger.LogInformation(
                "SMS sent successfully to {CellNumber}, type: {MessageType}",
                cellNumber,
                messageType);

            return new SmsResult
            {
                Success = true,
                SmsMessageId = smsMessage.SmsMessageId,
                ProviderReference = providerReference,
            };
        }
        catch (Exception ex)
        {
            smsMessage.Status = "Failed";
            smsMessage.FailureReason = ex.Message;

            await _smsMessageRepository.AddAsync(smsMessage, cancellationToken);

            _logger.LogError(ex, "Failed to send SMS to {CellNumber}", cellNumber);

            return new SmsResult
            {
                Success = false,
                SmsMessageId = smsMessage.SmsMessageId,
                ErrorMessage = ex.Message,
            };
        }
    }

    /// <inheritdoc/>
    public async Task<SmsResult> SendOtpSmsAsync(
        string cellNumber,
        string otpCode,
        CancellationToken cancellationToken = default)
    {
        var message = $"Your verification code is: {otpCode}. This code expires in 5 minutes.";

        return await SendSmsAsync(
            cellNumber,
            message,
            Models.Domain.SmsMessageType.Otp,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<SmsResult> SendPrizeNotificationAsync(
        string cellNumber,
        string prizeName,
        string? redemptionLink = null,
        DateTime? expiryDate = null,
        CancellationToken cancellationToken = default)
    {
        var message = $"Congratulations! You have won: {prizeName}.";

        if (!string.IsNullOrWhiteSpace(redemptionLink))
        {
            message += $" Redeem at: {redemptionLink}";
        }

        if (expiryDate.HasValue)
        {
            message += $" Expires: {expiryDate.Value:dd MMM yyyy}";
        }

        return await SendSmsAsync(
            cellNumber,
            message,
            Models.Domain.SmsMessageType.PrizeNotification,
            "PrizeAward",
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<SmsResult> SendRedemptionConfirmationAsync(
        string cellNumber,
        string prizeName,
        string? redemptionCode = null,
        CancellationToken cancellationToken = default)
    {
        var message = $"Your prize '{prizeName}' has been successfully redeemed.";

        if (!string.IsNullOrWhiteSpace(redemptionCode))
        {
            message += $" Reference: {redemptionCode}";
        }

        return await SendSmsAsync(
            cellNumber,
            message,
            Models.Domain.SmsMessageType.RedemptionConfirmation,
            "PrizeRedemption",
            cancellationToken: cancellationToken);
    }

    private static Task<string> SendToSmsProviderAsync(string cellNumber, string message, CancellationToken cancellationToken)
    {
        // TODO: Implement actual SMS provider integration (Twilio, Africa's Talking, etc.)
        // This is a placeholder that simulates successful SMS sending
        var providerReference = $"SMS-{Guid.NewGuid():N}"[..20];
        return Task.FromResult(providerReference);
    }
}
