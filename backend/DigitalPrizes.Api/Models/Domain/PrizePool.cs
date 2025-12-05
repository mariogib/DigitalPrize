namespace DigitalPrizes.Api.Models.Domain;

/// <summary>
/// Pool of prizes for grouping and management.
/// </summary>
public class PrizePool
{
    public int PrizePoolId { get; set; }
    public int? CompetitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual Competition? Competition { get; set; }
    public virtual ICollection<Prize> Prizes { get; set; } = new List<Prize>();
}
