using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Reports;

namespace DigitalPrizes.Api.Repositories.Interfaces;

/// <summary>
/// Repository interface for AuditLog entities.
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog, long>
{
    /// <summary>
    /// Gets audit logs with paging and filtering.
    /// </summary>
    Task<PagedResponse<AuditLog>> GetPagedAsync(
        AuditLogParameters parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs for a specific entity.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by subject (admin user).
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetBySubjectAsync(
        string subjectId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an action.
    /// </summary>
    Task LogAsync(
        string entityType,
        string entityId,
        string action,
        string? subjectId = null,
        string? subjectName = null,
        long? externalUserId = null,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? additionalData = null,
        CancellationToken cancellationToken = default);
}
