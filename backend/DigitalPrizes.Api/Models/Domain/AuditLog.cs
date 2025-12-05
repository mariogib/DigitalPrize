namespace DigitalPrizes.Api.Models.Domain;

/// <summary>
/// Audit log entry.
/// </summary>
public class AuditLog
{
    public long AuditLogId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, Award, Redeem, etc.
    public string? SubjectId { get; set; } // OAuth2 subject ID for admin actions
    public string? SubjectName { get; set; }
    public long? ExternalUserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? AdditionalData { get; set; } // JSON
}

/// <summary>
/// Audit action constants.
/// </summary>
public static class AuditAction
{
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Award = "Award";
    public const string Redeem = "Redeem";
    public const string BulkAward = "BulkAward";
    public const string Cancel = "Cancel";
    public const string Expire = "Expire";
    public const string SendSms = "SendSms";
    public const string VerifyOtp = "VerifyOtp";
    public const string Login = "Login";
    public const string Export = "Export";
}
