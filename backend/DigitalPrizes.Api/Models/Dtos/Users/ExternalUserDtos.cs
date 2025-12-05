namespace DigitalPrizes.Api.Models.Dtos.Users;

/// <summary>
/// Response DTO for external user.
/// </summary>
public record ExternalUserResponse
{
    public long ExternalUserId { get; init; }
    public string CellNumber { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool AcceptsMarketing { get; init; }
    public int RegistrationCount { get; init; }
    public int AwardsCount { get; init; }
    public int RedeemedCount { get; init; }
}

/// <summary>
/// Detailed response DTO for external user.
/// </summary>
public record ExternalUserDetailResponse : ExternalUserResponse
{
    public DateTime? LastActivityAt { get; init; }
    public List<UserAwardSummary> RecentAwards { get; init; } = new();
    public List<UserRegistrationSummary> Registrations { get; init; } = new();
}

/// <summary>
/// Summary of user's award.
/// </summary>
public record UserAwardSummary
{
    public long PrizeAwardId { get; init; }
    public string PrizeName { get; init; } = string.Empty;
    public DateTime AwardedAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? ExpiryDate { get; init; }
}

/// <summary>
/// Summary of user's registration.
/// </summary>
public record UserRegistrationSummary
{
    public long RegistrationId { get; init; }
    public int CompetitionId { get; init; }
    public string CompetitionName { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
}

/// <summary>
/// Request DTO for updating external user.
/// </summary>
public record UpdateExternalUserRequest
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public bool AcceptsMarketing { get; init; }
}

/// <summary>
/// Request DTO for looking up user by cell number.
/// </summary>
public record LookupUserRequest
{
    public string CellNumber { get; init; } = string.Empty;
}
