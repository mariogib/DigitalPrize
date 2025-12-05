namespace DigitalPrizes.Api.Models.Dtos.Prizes;

/// <summary>
/// Summary response DTO for prize pool.
/// </summary>
public record PrizePoolSummaryResponse
{
    public int PrizePoolId { get; init; }
    public int? CompetitionId { get; init; }
    public string? CompetitionName { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int TotalPrizes { get; init; }
    public int AvailablePrizes { get; init; }
    public int AwardedPrizes { get; init; }
    public int RedeemedPrizes { get; init; }
}

/// <summary>
/// Detailed response DTO for prize pool.
/// </summary>
public record PrizePoolDetailResponse : PrizePoolSummaryResponse
{
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<PrizeSummaryResponse> Prizes { get; init; } = new();
}

/// <summary>
/// Request DTO for creating a prize pool.
/// </summary>
public record CreatePrizePoolRequest
{
    public int? CompetitionId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

/// <summary>
/// Request DTO for updating a prize pool.
/// </summary>
public record UpdatePrizePoolRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}
