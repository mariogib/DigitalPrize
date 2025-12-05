using DigitalPrizes.Api.Data;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Reports;
using DigitalPrizes.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrizes.Api.Repositories.Implementations;

/// <summary>
/// EF Core implementation of IAuditLogRepository.
/// </summary>
public class AuditLogRepository : RepositoryBase<AuditLog, long>, IAuditLogRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public AuditLogRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<PagedResponse<AuditLog>> GetPagedAsync(
        AuditLogParameters parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.EntityType))
        {
            query = query.Where(a => a.EntityType == parameters.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Action))
        {
            query = query.Where(a => a.Action == parameters.Action);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SubjectId))
        {
            query = query.Where(a => a.SubjectId == parameters.SubjectId);
        }

        if (parameters.From.HasValue)
        {
            query = query.Where(a => a.Timestamp >= parameters.From.Value);
        }

        if (parameters.To.HasValue)
        {
            query = query.Where(a => a.Timestamp <= parameters.To.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm;
            query = query.Where(a => a.EntityId.Contains(searchTerm) ||
                                     (a.SubjectName != null && EF.Functions.Like(a.SubjectName, $"%{searchTerm}%")));
        }

        query = query.OrderByDescending(a => a.Timestamp);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AuditLog>> GetBySubjectAsync(
        string subjectId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(a => a.SubjectId == subjectId);

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= toDate.Value);
        }

        return await query
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task LogAsync(
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
        CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            SubjectId = subjectId,
            SubjectName = subjectName,
            ExternalUserId = externalUserId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            AdditionalData = additionalData,
            Timestamp = DateTime.UtcNow,
        };

        await AddAsync(log, cancellationToken);
    }
}
