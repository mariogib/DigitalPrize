namespace DigitalPrizes.Api.Models.Domain;

/// <summary>
/// Answer to a registration form field.
/// </summary>
public class RegistrationAnswer
{
    public long RegistrationAnswerId { get; set; }
    public long RegistrationId { get; set; }
    public int RegistrationFieldId { get; set; }
    public string? Value { get; set; }

    // Navigation properties
    public virtual Registration Registration { get; set; } = null!;
    public virtual RegistrationField RegistrationField { get; set; } = null!;
}
