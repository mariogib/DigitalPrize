namespace DigitalPrizes.Api.Models.Domain;

/// <summary>
/// Prize redemption record.
/// </summary>
public class PrizeRedemption
{
    public long PrizeRedemptionId { get; set; }
    public long PrizeAwardId { get; set; }
    public DateTime RedeemedAt { get; set; }
    public string? RedeemedChannel { get; set; } // WebPortal, Kiosk, API, POS
    public string? RedeemedFromIp { get; set; }
    public string? RedemptionStatus { get; set; } // Pending, Completed, Failed
    public string? Notes { get; set; }

    // Navigation properties
    public virtual PrizeAward PrizeAward { get; set; } = null!;
}

/// <summary>
/// Redemption channel constants.
/// </summary>
public static class RedemptionChannel
{
    public const string WebPortal = "WebPortal";
    public const string Kiosk = "Kiosk";
    public const string Api = "API";
    public const string Pos = "POS";
}

/// <summary>
/// Verification method constants.
/// </summary>
public static class VerificationMethod
{
    public const string Otp = "OTP";
    public const string Pin = "PIN";
    public const string None = "None";
}
