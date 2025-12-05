using DigitalPrizes.Api.Data;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Repositories.Interfaces;
using DigitalPrizes.Api.Services.Interfaces;

namespace DigitalPrizes.Api.Services.Implementations;

/// <summary>
/// Service implementation for audit logging.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditService"/> class.
    /// </summary>
    /// <param name="auditLogRepository">The audit log repository.</param>
    /// <param name="logger">The logger.</param>
    public AuditService(IAuditLogRepository auditLogRepository, ILogger<AuditService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task LogAsync(
        string action,
        string entityType,
        string entityId,
        string? subjectId = null,
        string? details = null,
        string? oldValues = null,
        string? newValues = null,
        CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            SubjectId = subjectId,
            Timestamp = DateTime.UtcNow,
            OldValues = oldValues,
            NewValues = newValues,
            AdditionalData = details,
        };

        await _auditLogRepository.AddAsync(log, cancellationToken);

        _logger.LogInformation(
            "Audit: {Action} on {EntityType}:{EntityId} by {SubjectId}",
            action,
            entityType,
            entityId,
            subjectId ?? "System");
    }

    /// <inheritdoc/>
    public async Task LogCompetitionActionAsync(
        string action,
        int competitionId,
        string? subjectId = null,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(
            action,
            "Competition",
            competitionId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            subjectId,
            details,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task LogPrizeAwardActionAsync(
        string action,
        long prizeAwardId,
        string? subjectId = null,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(
            action,
            "PrizeAward",
            prizeAwardId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            subjectId,
            details,
            cancellationToken: cancellationToken);
    }
}
