namespace DigitalPrizes.Api.Models.Domain;

/// <summary>
/// Prize award to an external user.
/// </summary>
public class PrizeAward
{
    public long PrizeAwardId { get; set; }
    public long PrizeId { get; set; }
    public int? CompetitionId { get; set; }
    public long? ExternalUserId { get; set; }
    public string CellNumber { get; set; } = string.Empty;
    public DateTime AwardedAt { get; set; }
    public string? AwardedBySubjectId { get; set; } // Admin subject/ID from OAuth2 server
    public string? AwardMethod { get; set; } // Manual, Bulk, Auto
    public string? NotificationChannel { get; set; } // SMS, WhatsApp, Email
    public string? NotificationStatus { get; set; } // Pending, Sent, Failed
    public string Status { get; set; } = "Awarded"; // Awarded, Redeemed, Expired, Cancelled
    public DateTime? ExpiryDate { get; set; }
    public string? ExternalReference { get; set; } // Wallet ref, 3rd party ref

    // Navigation properties
    public virtual Prize Prize { get; set; } = null!;
    public virtual Competition? Competition { get; set; }
    public virtual ExternalUser? ExternalUser { get; set; }
    public virtual PrizeRedemption? PrizeRedemption { get; set; }

    public bool IsRedeemable => Status == "Awarded" && (!ExpiryDate.HasValue || DateTime.UtcNow <= ExpiryDate.Value);
}

/// <summary>
/// Award status constants.
/// </summary>
public static class AwardStatus
{
    public const string Awarded = "Awarded";
    public const string Redeemed = "Redeemed";
    public const string Expired = "Expired";
    public const string Cancelled = "Cancelled";
}

/// <summary>
/// Award method constants.
/// </summary>
public static class AwardMethod
{
    public const string Manual = "Manual";
    public const string Bulk = "Bulk";
    public const string Auto = "Auto";
}

/// <summary>
/// Notification channel constants.
/// </summary>
public static class NotificationChannel
{
    public const string Sms = "SMS";
    public const string WhatsApp = "WhatsApp";
    public const string Email = "Email";
}

/// <summary>
/// Notification status constants.
/// </summary>
public static class NotificationStatus
{
    public const string Pending = "Pending";
    public const string Sent = "Sent";
    public const string Failed = "Failed";
}
