namespace DigitalPrizes.Api.Models.Dtos.Registrations;

/// <summary>
/// Response DTO for registration.
/// </summary>
public record RegistrationResponse
{
    public long RegistrationId { get; init; }
    public int CompetitionId { get; init; }
    public string CompetitionName { get; init; } = string.Empty;
    public long? ExternalUserId { get; init; }
    public string CellNumber { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
    public string RegistrationChannel { get; init; } = string.Empty;
    public List<RegistrationAnswerResponse> Answers { get; init; } = new();
}

/// <summary>
/// Response DTO for registration answer.
/// </summary>
public record RegistrationAnswerResponse
{
    public long RegistrationAnswerId { get; init; }
    public int RegistrationFieldId { get; init; }
    public string FieldName { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

/// <summary>
/// Request DTO for public registration (external user).
/// </summary>
public record PublicRegistrationRequest
{
    public int CompetitionId { get; init; }
    public string CellNumber { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public Dictionary<int, string> FieldAnswers { get; init; } = new();
    public bool AcceptsTerms { get; init; }
    public bool AcceptsMarketing { get; init; }
}

/// <summary>
/// Response DTO for public registration result.
/// </summary>
public record PublicRegistrationResponse
{
    public long RegistrationId { get; init; }
    public string CellNumber { get; init; } = string.Empty;
    public string Message { get; init; } = "Registration successful";
    public bool RequiresOtp { get; init; }
}

/// <summary>
/// Admin request DTO for creating registration on behalf of user.
/// </summary>
public record AdminRegistrationRequest
{
    public int CompetitionId { get; init; }
    public string CellNumber { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public Dictionary<int, string> FieldAnswers { get; init; } = new();
}

/// <summary>
/// Request DTO for verifying registration with OTP.
/// </summary>
public record VerifyRegistrationRequest
{
    public long RegistrationId { get; init; }
    public string Otp { get; init; } = string.Empty;
}

/// <summary>
/// Response DTO for registration verification.
/// </summary>
public record VerificationResponse
{
    public bool IsVerified { get; init; }
    public string Message { get; init; } = string.Empty;
    public long? RegistrationId { get; init; }
}

/// <summary>
/// Request DTO for resending OTP.
/// </summary>
public record ResendOtpRequest
{
    public long RegistrationId { get; init; }
}

/// <summary>
/// Response DTO for registration status check.
/// </summary>
public record RegistrationStatusResponse
{
    public long RegistrationId { get; init; }
    public int CompetitionId { get; init; }
    public string CellNumber { get; init; } = string.Empty;
    public bool IsVerified { get; init; }
    public DateTime RegisteredAt { get; init; }
    public string Status { get; init; } = string.Empty;
}
