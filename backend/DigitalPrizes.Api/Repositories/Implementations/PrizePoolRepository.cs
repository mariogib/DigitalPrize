using DigitalPrizes.Api.Data;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrizes.Api.Repositories.Implementations;

/// <summary>
/// EF Core implementation of IPrizePoolRepository.
/// </summary>
public class PrizePoolRepository : RepositoryBase<PrizePool, int>, IPrizePoolRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrizePoolRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public PrizePoolRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<PagedResponse<PrizePool>> GetPagedAsync(
        FilterParameters parameters,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var query = DbSet.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm;
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{searchTerm}%") ||
                                     (p.Description != null && EF.Functions.Like(p.Description, $"%{searchTerm}%")));
        }

        query = parameters.SortBy?.ToUpperInvariant() switch
        {
            "NAME" => parameters.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(p => p.Competition)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<PrizePool>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
        };
    }

    /// <inheritdoc />
    public async Task<PrizePool?> GetWithPrizesAsync(int prizePoolId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Competition)
            .Include(p => p.Prizes)
            .ThenInclude(pr => pr.PrizeType)
            .FirstOrDefaultAsync(p => p.PrizePoolId == prizePoolId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PrizePool>> GetActiveWithAvailablePrizesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Prizes.Where(pr => pr.IsActive && pr.RemainingQuantity > 0))
            .Where(p => p.IsActive && p.Prizes.Any(pr => pr.IsActive && pr.RemainingQuantity > 0))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }
}
