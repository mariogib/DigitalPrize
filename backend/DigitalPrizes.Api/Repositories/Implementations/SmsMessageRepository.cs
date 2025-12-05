using DigitalPrizes.Api.Data;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrizes.Api.Repositories.Implementations;

/// <summary>
/// EF Core implementation of ISmsMessageRepository.
/// </summary>
public class SmsMessageRepository : RepositoryBase<SmsMessage, long>, ISmsMessageRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SmsMessageRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public SmsMessageRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<PagedResponse<SmsMessage>> GetPagedAsync(
        FilterParameters parameters,
        string? status = null,
        string? messageType = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(m => m.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(messageType))
        {
            query = query.Where(m => m.MessageType == messageType);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm;
            query = query.Where(m => m.CellNumber.Contains(searchTerm) ||
                                     EF.Functions.Like(m.Message, $"%{searchTerm}%"));
        }

        query = query.OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<SmsMessage>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SmsMessage>> GetPendingAsync(
        int maxCount = 100,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(m => m.Status == SmsStatus.Pending)
            .OrderBy(m => m.CreatedAt)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SmsMessage>> GetFailedForRetryAsync(
        int maxRetries = 3,
        int maxCount = 100,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(m => m.Status == SmsStatus.Failed && m.RetryCount < maxRetries)
            .OrderBy(m => m.CreatedAt)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateStatusAsync(
        long smsMessageId,
        string status,
        string? providerReference = null,
        string? providerResponse = null,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        var message = await DbSet.FindAsync(new object[] { smsMessageId }, cancellationToken);
        if (message == null)
        {
            return;
        }

        message.Status = status;
        if (providerReference != null)
        {
            message.ProviderReference = providerReference;
        }

        if (providerResponse != null)
        {
            message.ProviderResponse = providerResponse;
        }

        if (failureReason != null)
        {
            message.FailureReason = failureReason;
        }

        if (status == SmsStatus.Sent)
        {
            message.SentAt = DateTime.UtcNow;
        }
        else if (status == SmsStatus.Delivered)
        {
            message.DeliveredAt = DateTime.UtcNow;
        }
        else if (status == SmsStatus.Failed)
        {
            message.RetryCount++;
        }

        await SaveChangesAsync(cancellationToken);
    }
}
