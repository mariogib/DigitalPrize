namespace DigitalPrizes.Api.Models.Domain;

/// <summary>
/// Competition entity for managing promotions and contests.
/// </summary>
public class Competition
{
    public int CompetitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TermsUrl { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? BannerImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<RegistrationField> RegistrationFields { get; set; } = new List<RegistrationField>();
    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    public virtual ICollection<PrizeAward> PrizeAwards { get; set; } = new List<PrizeAward>();
    public virtual PrizePool? PrizePool { get; set; }

    public bool IsCurrentlyActive => IsActive && DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
}

/// <summary>
/// Competition status constants.
/// </summary>
public static class CompetitionStatus
{
    public const string Draft = "Draft";
    public const string Active = "Active";
    public const string Paused = "Paused";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}
