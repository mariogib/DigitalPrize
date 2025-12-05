namespace DigitalPrizes.Api.Models.Domain;

/// <summary>
/// External user identified by cell number.
/// </summary>
public class ExternalUser
{
    public long ExternalUserId { get; set; }
    public string CellNumber { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    public virtual ICollection<PrizeAward> PrizeAwards { get; set; } = new List<PrizeAward>();
}
