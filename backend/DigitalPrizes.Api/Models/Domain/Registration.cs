namespace DigitalPrizes.Api.Models.Domain;

/// <summary>
/// Competition registration submission.
/// </summary>
public class Registration
{
    public long RegistrationId { get; set; }
    public int CompetitionId { get; set; }
    public long? ExternalUserId { get; set; }
    public string CellNumber { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public string? Source { get; set; } // Web, Import, API, SMS
    public bool ConsentGiven { get; set; }

    // Navigation properties
    public virtual Competition Competition { get; set; } = null!;
    public virtual ExternalUser? ExternalUser { get; set; }
    public virtual ICollection<RegistrationAnswer> RegistrationAnswers { get; set; } = new List<RegistrationAnswer>();
}

/// <summary>
/// Registration channel constants.
/// </summary>
public static class RegistrationChannelType
{
    public const string Web = "Web";
    public const string Import = "Import";
    public const string Api = "API";
    public const string Sms = "SMS";
}
