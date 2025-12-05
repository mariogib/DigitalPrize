namespace DigitalPrizes.Api.Models.Dtos;

/// <summary>
/// Data transfer object for updating an existing prize.
/// </summary>
public class UpdatePrizeDto
{
    /// <summary>
    /// Gets or sets the prize name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the prize description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the prize monetary value.
    /// </summary>
    public decimal? MonetaryValue { get; set; }
}
