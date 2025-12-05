namespace DigitalPrizes.Api.Models.Dtos;

/// <summary>
/// Data transfer object for prize information.
/// </summary>
public class PrizeDto
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the prize name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the prize description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the prize value.
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Gets or sets the prize status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the prize was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the prize was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
