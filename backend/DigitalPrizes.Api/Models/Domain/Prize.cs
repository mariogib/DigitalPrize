namespace DigitalPrizes.Api.Models.Domain;

/// <summary>
/// Prize definition entity.
/// </summary>
public class Prize
{
    public long PrizeId { get; set; }
    public int PrizePoolId { get; set; }
    public int PrizeTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? MonetaryValue { get; set; }
    public int TotalQuantity { get; set; }
    public int RemainingQuantity { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? ImageUrl { get; set; }
    public string? MetadataJson { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual PrizePool PrizePool { get; set; } = null!;
    public virtual PrizeType PrizeType { get; set; } = null!;
    public virtual ICollection<PrizeAward> PrizeAwards { get; set; } = new List<PrizeAward>();

    public bool IsExpired => ExpiryDate.HasValue && DateTime.UtcNow > ExpiryDate.Value;
    public bool HasAvailableQuantity => RemainingQuantity > 0;
    public bool IsCurrentlyValid => IsActive && HasAvailableQuantity && !IsExpired;
}
