namespace DigitalPrizes.Api.Models.Dtos.Prizes;

/// <summary>
/// Summary response DTO for prize.
/// </summary>
public record PrizeSummaryResponse
{
    public long PrizeId { get; init; }
    public int PrizePoolId { get; init; }
    public string PrizePoolName { get; init; } = string.Empty;
    public int PrizeTypeId { get; init; }
    public string PrizeTypeName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal? MonetaryValue { get; init; }
    public int TotalQuantity { get; init; }
    public int RemainingQuantity { get; init; }
    public int AwardedQuantity { get; init; }
    public int RedeemedQuantity { get; init; }
    public bool IsActive { get; init; }
    public DateTime? ExpiryDate { get; init; }
}

/// <summary>
/// Detailed response DTO for prize.
/// </summary>
public record PrizeDetailResponse : PrizeSummaryResponse
{
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public string? MetadataJson { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Request DTO for creating a prize.
/// </summary>
public record CreatePrizeRequest
{
    public int PrizePoolId { get; init; }
    public int PrizeTypeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal? MonetaryValue { get; init; }
    public int TotalQuantity { get; init; } = 1;
    public string? ImageUrl { get; init; }
    public string? MetadataJson { get; init; }
    public DateTime? ExpiryDate { get; init; }
}

/// <summary>
/// Request DTO for updating a prize.
/// </summary>
public record UpdatePrizeRequest
{
    public int PrizeTypeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal? MonetaryValue { get; init; }
    public int TotalQuantity { get; init; }
    public string? ImageUrl { get; init; }
    public string? MetadataJson { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// Request DTO for bulk creating prizes.
/// </summary>
public record BulkCreatePrizesRequest
{
    public int PrizePoolId { get; init; }
    public int PrizeTypeId { get; init; }
    public string NamePrefix { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal? MonetaryValue { get; init; }
    public int Quantity { get; init; }
    public DateTime? ExpiryDate { get; init; }
}
