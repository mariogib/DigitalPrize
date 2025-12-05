using DigitalPrizes.Api.Data;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrizes.Api.Repositories.Implementations;

/// <summary>
/// EF Core implementation of IPrizeRedemptionRepository.
/// </summary>
public class PrizeRedemptionRepository : RepositoryBase<PrizeRedemption, long>, IPrizeRedemptionRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrizeRedemptionRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public PrizeRedemptionRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<PagedResponse<PrizeRedemption>> GetPagedAsync(
        FilterParameters parameters,
        int? competitionId = null,
        int? prizePoolId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var query = DbSet
            .Include(r => r.PrizeAward)
            .ThenInclude(a => a.Prize)
            .ThenInclude(p => p.PrizeType)
            .AsQueryable();

        if (competitionId.HasValue)
        {
            query = query.Where(r => r.PrizeAward.CompetitionId == competitionId.Value);
        }

        if (prizePoolId.HasValue)
        {
            query = query.Where(r => r.PrizeAward.Prize.PrizePoolId == prizePoolId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm;
            query = query.Where(r => r.PrizeAward.CellNumber.Contains(searchTerm) ||
                                     r.PrizeAward.Prize.Name.Contains(searchTerm));
        }

        query = parameters.SortBy?.ToUpperInvariant() switch
        {
            "CELLNUMBER" => parameters.SortDescending ? query.OrderByDescending(r => r.PrizeAward.CellNumber) : query.OrderBy(r => r.PrizeAward.CellNumber),
            "PRIZENAME" => parameters.SortDescending ? query.OrderByDescending(r => r.PrizeAward.Prize.Name) : query.OrderBy(r => r.PrizeAward.Prize.Name),
            _ => query.OrderByDescending(r => r.RedeemedAt),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<PrizeRedemption>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
        };
    }

    /// <inheritdoc />
    public async Task<PrizeRedemption?> GetByPrizeAwardIdAsync(
        long prizeAwardId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.PrizeAward)
            .ThenInclude(a => a.Prize)
            .FirstOrDefaultAsync(r => r.PrizeAwardId == prizeAwardId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PrizeRedemption?> GetWithRelationsAsync(
        long prizeRedemptionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.PrizeAward)
            .ThenInclude(a => a.Prize)
            .ThenInclude(p => p.PrizeType)
            .FirstOrDefaultAsync(r => r.PrizeRedemptionId == prizeRedemptionId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PrizeRedemption>> GetByCellNumberAsync(
        string cellNumber,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.PrizeAward)
            .ThenInclude(a => a.Prize)
            .ThenInclude(p => p.PrizeType)
            .Where(r => r.PrizeAward.CellNumber == cellNumber)
            .OrderByDescending(r => r.RedeemedAt)
            .ToListAsync(cancellationToken);
    }
}
