using DigitalPrizes.Api.Models.Dtos.Prizes;

namespace DigitalPrizes.Api.Models.Dtos.Competitions;

/// <summary>
/// Response DTO for competition.
/// </summary>
public record CompetitionResponse
{
    public int CompetitionId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public int RegistrationCount { get; init; }
    public int PrizePoolCount { get; init; }
    public int AwardedPrizesCount { get; init; }
}

/// <summary>
/// Request DTO for creating a competition.
/// </summary>
public record CreateCompetitionRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public List<CreateRegistrationFieldRequest>? RegistrationFields { get; init; }
}

/// <summary>
/// Request DTO for updating a competition.
/// </summary>
public record UpdateCompetitionRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Status { get; init; } = string.Empty;
}

/// <summary>
/// Detailed competition response with registration fields.
/// </summary>
public record CompetitionDetailResponse : CompetitionResponse
{
    public List<RegistrationFieldResponse> RegistrationFields { get; init; } = new();
    public List<PrizePoolSummaryResponse> PrizePools { get; init; } = new();
}
