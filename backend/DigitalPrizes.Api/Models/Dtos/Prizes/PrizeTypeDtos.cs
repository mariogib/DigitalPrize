namespace DigitalPrizes.Api.Models.Dtos.Prizes;

/// <summary>
/// Response DTO for prize type.
/// </summary>
public record PrizeTypeResponse
{
    public int PrizeTypeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

/// <summary>
/// Request DTO for creating a prize type.
/// </summary>
public record CreatePrizeTypeRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

/// <summary>
/// Request DTO for updating a prize type.
/// </summary>
public record UpdatePrizeTypeRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
