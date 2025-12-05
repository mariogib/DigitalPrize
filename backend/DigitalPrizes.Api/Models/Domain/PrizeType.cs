namespace DigitalPrizes.Api.Models.Domain;

/// <summary>
/// Prize type classification.
/// </summary>
public class PrizeType
{
    public int PrizeTypeId { get; set; }
    public string Name { get; set; } = string.Empty; // VoucherCode, QRCode, UrlLink, DiscountCode, WalletCredit
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<Prize> Prizes { get; set; } = new List<Prize>();
}

/// <summary>
/// Prize type name constants.
/// </summary>
public static class PrizeTypeName
{
    public const string VoucherCode = "VoucherCode";
    public const string QrCode = "QRCode";
    public const string UrlLink = "UrlLink";
    public const string DiscountCode = "DiscountCode";
    public const string WalletCredit = "WalletCredit";
    public const string Physical = "Physical";
}
