namespace DigitalPrizes.Api.Models.Dtos;

/// <summary>
/// Data transfer object for creating a new prize.
/// </summary>
public class CreatePrizeDto
{
    /// <summary>
    /// Gets or sets the prize name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the prize description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the prize monetary value.
    /// </summary>
    public decimal? MonetaryValue { get; set; }
}
