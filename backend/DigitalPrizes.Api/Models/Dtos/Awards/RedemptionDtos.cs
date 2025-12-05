namespace DigitalPrizes.Api.Models.Dtos.Awards;

/// <summary>
/// Response DTO for prize redemption.
/// </summary>
public record PrizeRedemptionResponse
{
    public long PrizeRedemptionId { get; init; }
    public long PrizeAwardId { get; init; }
    public DateTime RedeemedAt { get; init; }
    public string? RedeemedChannel { get; init; }
    public string? RedeemedFromIp { get; init; }
    public string? RedemptionStatus { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Request DTO for initiating prize redemption.
/// </summary>
public record InitiateRedemptionRequest
{
    /// <summary>Gets the cell number of the winner.</summary>
    public string CellNumber { get; init; } = string.Empty;

    /// <summary>Gets the prize award ID to redeem.</summary>
    public long? PrizeAwardId { get; init; }

    /// <summary>Gets the redemption channel.</summary>
    public string RedemptionChannel { get; init; } = "WebPortal";
}

/// <summary>
/// Response DTO for redemption initiation.
/// </summary>
public record InitiateRedemptionResponse
{
    public bool RequiresOtp { get; init; } = true;
    public string? Message { get; init; }
    public List<RedeemablePrizeResponse> RedeemablePrizes { get; init; } = new();
}

/// <summary>
/// Response DTO for redeemable prize.
/// </summary>
public record RedeemablePrizeResponse
{
    public long PrizeAwardId { get; init; }
    public string PrizeName { get; init; } = string.Empty;
    public string PrizeTypeName { get; init; } = string.Empty;
    public decimal? MonetaryValue { get; init; }
    public DateTime AwardedAt { get; init; }
    public DateTime? ExpiryDate { get; init; }
}

/// <summary>
/// Request DTO for completing prize redemption.
/// </summary>
public record CompleteRedemptionRequest
{
    /// <summary>Gets the prize award ID.</summary>
    public long PrizeAwardId { get; init; }

    /// <summary>Gets the cell number of the winner.</summary>
    public string CellNumber { get; init; } = string.Empty;

    /// <summary>Gets the OTP code.</summary>
    public string OtpCode { get; init; } = string.Empty;

    /// <summary>Gets the redemption channel.</summary>
    public string RedemptionChannel { get; init; } = "WebPortal";

    /// <summary>Gets optional notes.</summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Response DTO for completed redemption.
/// </summary>
public record CompleteRedemptionResponse
{
    public bool Success { get; init; }
    public long? PrizeRedemptionId { get; init; }
    public string? RedemptionCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public RedemptionConfirmation? Confirmation { get; init; }
}

/// <summary>
/// Redemption confirmation details.
/// </summary>
public record RedemptionConfirmation
{
    public string PrizeName { get; init; } = string.Empty;
    public decimal? MonetaryValue { get; init; }
    public DateTime RedeemedAt { get; init; }
    public string? ExternalReference { get; init; }
}

/// <summary>
/// Response DTO for available prizes.
/// </summary>
public record AvailablePrizesResponse
{
    /// <summary>Gets the cell number.</summary>
    public string CellNumber { get; init; } = string.Empty;

    /// <summary>Gets the list of available prizes.</summary>
    public List<AvailablePrizeDto> Prizes { get; init; } = new();
}

/// <summary>
/// DTO for an available prize.
/// </summary>
public record AvailablePrizeDto
{
    /// <summary>Gets the award ID.</summary>
    public long AwardId { get; init; }

    /// <summary>Gets the prize name.</summary>
    public string PrizeName { get; init; } = string.Empty;

    /// <summary>Gets the competition name.</summary>
    public string CompetitionName { get; init; } = string.Empty;

    /// <summary>Gets the date the prize was awarded.</summary>
    public DateTime AwardedDate { get; init; }

    /// <summary>Gets the expiry date.</summary>
    public DateTime? ExpiryDate { get; init; }
}

/// <summary>
/// Response DTO for redemption initiation (simplified for public API).
/// </summary>
public record RedemptionInitiationResponse
{
    /// <summary>Gets the redemption ID.</summary>
    public long RedemptionId { get; init; }

    /// <summary>Gets the award ID.</summary>
    public long AwardId { get; init; }

    /// <summary>Gets the message.</summary>
    public string Message { get; init; } = "OTP sent to your phone";

    /// <summary>Gets whether OTP is required.</summary>
    public bool RequiresOtp { get; init; } = true;
}

/// <summary>
/// Response DTO for completed redemption (simplified).
/// </summary>
public record RedemptionResponse
{
    /// <summary>Gets the redemption ID.</summary>
    public long RedemptionId { get; init; }

    /// <summary>Gets the award ID.</summary>
    public long AwardId { get; init; }

    /// <summary>Gets the prize name.</summary>
    public string PrizeName { get; init; } = string.Empty;

    /// <summary>Gets the status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets when the prize was redeemed.</summary>
    public DateTime? RedeemedAt { get; init; }

    /// <summary>Gets the message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets the voucher code if applicable.</summary>
    public string? VoucherCode { get; init; }
}

/// <summary>
/// Response DTO for redemption status.
/// </summary>
public record RedemptionStatusResponse
{
    /// <summary>Gets the redemption ID.</summary>
    public long RedemptionId { get; init; }

    /// <summary>Gets the award ID.</summary>
    public long AwardId { get; init; }

    /// <summary>Gets the prize name.</summary>
    public string PrizeName { get; init; } = string.Empty;

    /// <summary>Gets the status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets when the redemption was initiated.</summary>
    public DateTime InitiatedAt { get; init; }

    /// <summary>Gets when the redemption was completed.</summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>Gets when the OTP expires.</summary>
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>
/// Request DTO for resending redemption OTP.
/// </summary>
public record ResendRedemptionOtpRequest
{
    /// <summary>Gets the redemption ID.</summary>
    public long RedemptionId { get; init; }
}
