using DigitalPrizes.Api.Data;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Reports;
using DigitalPrizes.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrizes.Api.Repositories.Implementations;

/// <summary>
/// EF Core implementation of IPrizeAwardRepository.
/// </summary>
public class PrizeAwardRepository : RepositoryBase<PrizeAward, long>, IPrizeAwardRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrizeAwardRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public PrizeAwardRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Models.Dtos.Common.PagedResponse<PrizeAward>> GetPagedAsync(
        AwardsReportParameters parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var query = DbSet
            .Include(a => a.Prize)
            .ThenInclude(p => p.PrizeType)
            .Include(a => a.Competition)
            .Include(a => a.ExternalUser)
            .AsQueryable();

        if (parameters.CompetitionId.HasValue)
        {
            query = query.Where(a => a.CompetitionId == parameters.CompetitionId.Value);
        }

        if (parameters.PrizePoolId.HasValue)
        {
            query = query.Where(a => a.Prize.PrizePoolId == parameters.PrizePoolId.Value);
        }

        if (parameters.PrizeTypeId.HasValue)
        {
            query = query.Where(a => a.Prize.PrizeTypeId == parameters.PrizeTypeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            query = query.Where(a => a.Status == parameters.Status);
        }

        if (parameters.AwardedFrom.HasValue)
        {
            query = query.Where(a => a.AwardedAt >= parameters.AwardedFrom.Value);
        }

        if (parameters.AwardedTo.HasValue)
        {
            query = query.Where(a => a.AwardedAt <= parameters.AwardedTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm;
            query = query.Where(a => a.CellNumber.Contains(searchTerm) ||
                                     EF.Functions.Like(a.Prize.Name, $"%{searchTerm}%"));
        }

        query = parameters.SortBy?.ToUpperInvariant() switch
        {
            "CELLNUMBER" => parameters.SortDescending ? query.OrderByDescending(a => a.CellNumber) : query.OrderBy(a => a.CellNumber),
            "PRIZE" => parameters.SortDescending ? query.OrderByDescending(a => a.Prize.Name) : query.OrderBy(a => a.Prize.Name),
            "STATUS" => parameters.SortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            _ => query.OrderByDescending(a => a.AwardedAt),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new Models.Dtos.Common.PagedResponse<PrizeAward>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
        };
    }

    /// <inheritdoc />
    public async Task<PrizeAward?> GetWithRelationsAsync(long prizeAwardId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Prize)
            .ThenInclude(p => p.PrizeType)
            .Include(a => a.Competition)
            .Include(a => a.ExternalUser)
            .Include(a => a.PrizeRedemption)
            .FirstOrDefaultAsync(a => a.PrizeAwardId == prizeAwardId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PrizeAward>> GetByCellNumberAsync(
        string cellNumber,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Prize)
            .ThenInclude(p => p.PrizeType)
            .Include(a => a.Competition)
            .Where(a => a.CellNumber == cellNumber)
            .OrderByDescending(a => a.AwardedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PrizeAward>> GetRedeemableAsync(
        string cellNumber,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Include(a => a.Prize)
            .ThenInclude(p => p.PrizeType)
            .Where(a => a.CellNumber == cellNumber &&
                        a.Status == AwardStatus.Awarded &&
                        (!a.ExpiryDate.HasValue || a.ExpiryDate.Value >= now))
            .OrderByDescending(a => a.AwardedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PrizeAward>> GetByCompetitionAsync(
        int competitionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Prize)
            .ThenInclude(p => p.PrizeType)
            .Include(a => a.ExternalUser)
            .Where(a => a.CompetitionId == competitionId)
            .OrderByDescending(a => a.AwardedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AwardStatistics> GetStatisticsAsync(
        int? competitionId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(a => a.Prize)
            .AsQueryable();

        if (competitionId.HasValue)
        {
            query = query.Where(a => a.CompetitionId == competitionId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.AwardedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.AwardedAt <= toDate.Value);
        }

        var awards = await query.ToListAsync(cancellationToken);

        return new AwardStatistics
        {
            TotalAwarded = awards.Count,
            TotalRedeemed = awards.Count(a => a.Status == AwardStatus.Redeemed),
            TotalExpired = awards.Count(a => a.Status == AwardStatus.Expired),
            TotalCancelled = awards.Count(a => a.Status == AwardStatus.Cancelled),
            PendingRedemption = awards.Count(a => a.Status == AwardStatus.Awarded && a.IsRedeemable),
            TotalValueAwarded = awards.Sum(a => a.Prize.MonetaryValue ?? 0),
            TotalValueRedeemed = awards.Where(a => a.Status == AwardStatus.Redeemed).Sum(a => a.Prize.MonetaryValue ?? 0),
        };
    }
}
