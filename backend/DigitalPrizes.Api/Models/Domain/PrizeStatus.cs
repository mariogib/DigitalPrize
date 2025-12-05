namespace DigitalPrizes.Api.Models.Domain;

/// <summary>
/// Represents the status of a prize.
/// </summary>
public enum PrizeStatus
{
    /// <summary>
    /// Prize is active and available.
    /// </summary>
    Active,

    /// <summary>
    /// Prize is inactive and not available.
    /// </summary>
    Inactive,

    /// <summary>
    /// Prize has been redeemed.
    /// </summary>
    Redeemed,
}
