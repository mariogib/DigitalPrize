namespace DigitalPrizes.Api.Models.Domain;

/// <summary>
/// SMS message record.
/// </summary>
public class SmsMessage
{
    public long SmsMessageId { get; set; }
    public string CellNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty; // OTP, PrizeNotification, Reminder
    public long? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; } // PrizeAward, Otp, Registration
    public string Status { get; set; } = "Pending"; // Pending, Sent, Delivered, Failed
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ProviderReference { get; set; }
    public string? ProviderResponse { get; set; }
    public string? FailureReason { get; set; }
    public int RetryCount { get; set; }
}

/// <summary>
/// SMS message type constants.
/// </summary>
public static class SmsMessageType
{
    /// <summary>OTP message.</summary>
    public const string Otp = "OTP";

    /// <summary>Prize notification message.</summary>
    public const string PrizeNotification = "PrizeNotification";

    /// <summary>Redemption confirmation message.</summary>
    public const string RedemptionConfirmation = "RedemptionConfirmation";

    /// <summary>Reminder message.</summary>
    public const string Reminder = "Reminder";

    /// <summary>Marketing message.</summary>
    public const string Marketing = "Marketing";
}

/// <summary>
/// SMS status constants.
/// </summary>
public static class SmsStatus
{
    public const string Pending = "Pending";
    public const string Sent = "Sent";
    public const string Delivered = "Delivered";
    public const string Failed = "Failed";
}
