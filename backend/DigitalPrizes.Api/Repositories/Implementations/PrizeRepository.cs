using DigitalPrizes.Api.Data;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrizes.Api.Repositories.Implementations;

/// <summary>
/// EF Core implementation of IPrizeRepository.
/// </summary>
public class PrizeRepository : RepositoryBase<Prize, long>, IPrizeRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrizeRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public PrizeRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<PagedResponse<Prize>> GetPagedAsync(
        FilterParameters parameters,
        int? prizePoolId = null,
        int? prizeTypeId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var query = DbSet
            .Include(p => p.PrizePool)
            .Include(p => p.PrizeType)
            .AsQueryable();

        if (prizePoolId.HasValue)
        {
            query = query.Where(p => p.PrizePoolId == prizePoolId.Value);
        }

        if (prizeTypeId.HasValue)
        {
            query = query.Where(p => p.PrizeTypeId == prizeTypeId.Value);
        }

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
            "VALUE" => parameters.SortDescending ? query.OrderByDescending(p => p.MonetaryValue) : query.OrderBy(p => p.MonetaryValue),
            _ => query.OrderByDescending(p => p.CreatedAt),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<Prize>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
        };
    }

    /// <inheritdoc />
    public async Task<Prize?> GetWithRelationsAsync(long prizeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.PrizePool)
            .Include(p => p.PrizeType)
            .FirstOrDefaultAsync(p => p.PrizeId == prizeId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Prize>> GetByPrizePoolAsync(
        int prizePoolId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.PrizeType)
            .Where(p => p.PrizePoolId == prizePoolId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Prize>> GetAvailableForAwardAsync(
        int? prizePoolId = null,
        int? prizeTypeId = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var query = DbSet
            .Include(p => p.PrizeType)
            .Where(p => p.IsActive &&
                        p.RemainingQuantity > 0 &&
                        (!p.ExpiryDate.HasValue || p.ExpiryDate.Value >= now));

        if (prizePoolId.HasValue)
        {
            query = query.Where(p => p.PrizePoolId == prizePoolId.Value);
        }

        if (prizeTypeId.HasValue)
        {
            query = query.Where(p => p.PrizeTypeId == prizeTypeId.Value);
        }

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Prize?> GetNextAvailableAsync(
        int prizePoolId,
        int? prizeTypeId = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var query = DbSet
            .Where(p => p.PrizePoolId == prizePoolId &&
                        p.IsActive &&
                        p.RemainingQuantity > 0 &&
                        (!p.ExpiryDate.HasValue || p.ExpiryDate.Value >= now));

        if (prizeTypeId.HasValue)
        {
            query = query.Where(p => p.PrizeTypeId == prizeTypeId.Value);
        }

        return await query
            .OrderBy(p => p.PrizeId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DecrementAvailableQuantityAsync(long prizeId, CancellationToken cancellationToken = default)
    {
        var result = await Context.Prizes
            .Where(p => p.PrizeId == prizeId && p.RemainingQuantity > 0)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(p => p.RemainingQuantity, p => p.RemainingQuantity - 1),
                cancellationToken);

        return result > 0;
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Prize> Prizes, int TotalCount)> GetPagedTupleAsync(
        int pageNumber,
        int pageSize,
        int? prizePoolId = null,
        int? prizeTypeId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(p => p.PrizePool)
            .Include(p => p.PrizeType)
            .AsQueryable();

        if (prizePoolId.HasValue)
        {
            query = query.Where(p => p.PrizePoolId == prizePoolId.Value);
        }

        if (prizeTypeId.HasValue)
        {
            query = query.Where(p => p.PrizeTypeId == prizeTypeId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<Prize?> GetByIdWithDetailsAsync(long prizeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.PrizePool)
            .Include(p => p.PrizeType)
            .Include(p => p.PrizeAwards)
            .FirstOrDefaultAsync(p => p.PrizeId == prizeId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<Prize> prizes, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(prizes, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }
}
