namespace DigitalPrizes.Api.Models.Domain;

/// <summary>
/// One-time password for verification.
/// </summary>
public class Otp
{
    public long OtpId { get; set; }
    public string CellNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty; // Redemption, Registration, Login
    public long? RelatedEntityId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; } = 3;

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired && AttemptCount < MaxAttempts;
}

/// <summary>
/// OTP purpose constants.
/// </summary>
public static class OtpPurpose
{
    public const string Redemption = "Redemption";
    public const string Registration = "Registration";
    public const string Login = "Login";
    public const string Verification = "Verification";
}
