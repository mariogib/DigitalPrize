namespace DigitalPrizes.Api.Models.Dtos.Competitions;

/// <summary>
/// Response DTO for registration field.
/// </summary>
public record RegistrationFieldResponse
{
    public int RegistrationFieldId { get; init; }
    public int CompetitionId { get; init; }
    public string FieldName { get; init; } = string.Empty;
    public string FieldType { get; init; } = string.Empty;
    public bool IsRequired { get; init; }
    public int DisplayOrder { get; init; }
    public string? Options { get; init; }
    public string? ValidationRules { get; init; }
}

/// <summary>
/// Request DTO for creating a registration field.
/// </summary>
public record CreateRegistrationFieldRequest
{
    public string FieldName { get; init; } = string.Empty;
    public string FieldType { get; init; } = "Text";
    public bool IsRequired { get; init; } = true;
    public int DisplayOrder { get; init; }
    public string? Options { get; init; }
    public string? ValidationRules { get; init; }
}

/// <summary>
/// Request DTO for updating a registration field.
/// </summary>
public record UpdateRegistrationFieldRequest
{
    public string FieldName { get; init; } = string.Empty;
    public string FieldType { get; init; } = string.Empty;
    public bool IsRequired { get; init; }
    public int DisplayOrder { get; init; }
    public string? Options { get; init; }
    public string? ValidationRules { get; init; }
}
