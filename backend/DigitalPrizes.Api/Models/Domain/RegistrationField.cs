namespace DigitalPrizes.Api.Models.Domain;

/// <summary>
/// Dynamic registration form field definition.
/// </summary>
public class RegistrationField
{
    public int RegistrationFieldId { get; set; }
    public int CompetitionId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FieldType { get; set; } = FieldTypes.Text;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? OptionsJson { get; set; } // JSON for dropdown options
    public string? ValidationRules { get; set; } // JSON for validation rules
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Competition Competition { get; set; } = null!;
    public virtual ICollection<RegistrationAnswer> RegistrationAnswers { get; set; } = new List<RegistrationAnswer>();
}

/// <summary>
/// Field type constants.
/// </summary>
public static class FieldTypes
{
    public const string Text = "Text";
    public const string Email = "Email";
    public const string Phone = "Phone";
    public const string Date = "Date";
    public const string Dropdown = "Dropdown";
    public const string Checkbox = "Checkbox";
    public const string Radio = "Radio";
    public const string TextArea = "TextArea";
}
