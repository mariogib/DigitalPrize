namespace DigitalPrizes.Api.Models.Dtos.Awards;

/// <summary>
/// Response DTO for prize award.
/// </summary>
public record PrizeAwardResponse
{
    public long PrizeAwardId { get; init; }
    public long PrizeId { get; init; }
    public string PrizeName { get; init; } = string.Empty;
    public string PrizeTypeName { get; init; } = string.Empty;
    public int? CompetitionId { get; init; }
    public string? CompetitionName { get; init; }
    public long? ExternalUserId { get; init; }
    public string CellNumber { get; init; } = string.Empty;
    public string? UserName { get; init; }
    public DateTime AwardedAt { get; init; }
    public string? AwardMethod { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? ExpiryDate { get; init; }
    public string? NotificationChannel { get; init; }
    public string? NotificationStatus { get; init; }
    public bool IsRedeemable { get; init; }
}

/// <summary>
/// Detailed response DTO for prize award.
/// </summary>
public record PrizeAwardDetailResponse : PrizeAwardResponse
{
    public string? AwardedBySubjectId { get; init; }
    public string? ExternalReference { get; init; }
    public decimal? PrizeValue { get; init; }
    public PrizeRedemptionResponse? Redemption { get; init; }
}

/// <summary>
/// Request DTO for awarding a prize to a user.
/// </summary>
public record AwardPrizeRequest
{
    public long PrizeId { get; init; }
    public string CellNumber { get; init; } = string.Empty;
    public int? CompetitionId { get; init; }
    public string? NotificationChannel { get; init; } = "SMS";
    public bool SendNotification { get; init; } = true;
    public int? ExpiryDays { get; init; }
    public string? ExternalReference { get; init; }
}

/// <summary>
/// Request DTO for bulk awarding prizes.
/// </summary>
public record BulkAwardPrizesRequest
{
    public int PrizePoolId { get; init; }
    public int? PrizeTypeId { get; init; }
    public int? CompetitionId { get; init; }
    public List<string> CellNumbers { get; init; } = new();
    public string? NotificationChannel { get; init; } = "SMS";
    public bool SendNotification { get; init; } = true;
    public int? ExpiryDays { get; init; }
}

/// <summary>
/// Response DTO for bulk award operation.
/// </summary>
public record BulkAwardResponse
{
    public int TotalRequested { get; init; }
    public int SuccessfulAwards { get; init; }
    public int FailedAwards { get; init; }
    public List<BulkAwardItemResult> Results { get; init; } = new();
}

/// <summary>
/// Result for individual bulk award item.
/// </summary>
public record BulkAwardItemResult
{
    public string CellNumber { get; init; } = string.Empty;
    public bool Success { get; init; }
    public long? PrizeAwardId { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Request DTO for cancelling an award.
/// </summary>
public record CancelAwardRequest
{
    public string Reason { get; init; } = string.Empty;
}
