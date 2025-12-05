namespace DigitalPrizes.Api.Models.Dtos.Otp;

/// <summary>
/// Request DTO for sending OTP.
/// </summary>
public record SendOtpRequest
{
    public string CellNumber { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public long? RelatedEntityId { get; init; }
}

/// <summary>
/// Response DTO for OTP send.
/// </summary>
public record SendOtpResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int? ExpiresInSeconds { get; init; }
}

/// <summary>
/// Request DTO for verifying OTP.
/// </summary>
public record VerifyOtpRequest
{
    public string CellNumber { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public long? RelatedEntityId { get; init; }
}

/// <summary>
/// Response DTO for OTP verification.
/// </summary>
public record VerifyOtpResponse
{
    public bool IsValid { get; init; }
    public string Message { get; init; } = string.Empty;
    public long? OtpId { get; init; }
    public int? RemainingAttempts { get; init; }
}
