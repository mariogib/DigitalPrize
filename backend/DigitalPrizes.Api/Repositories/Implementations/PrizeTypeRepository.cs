using DigitalPrizes.Api.Data;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrizes.Api.Repositories.Implementations;

/// <summary>
/// EF Core implementation of IPrizeTypeRepository.
/// </summary>
public class PrizeTypeRepository : RepositoryBase<PrizeType, int>, IPrizeTypeRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrizeTypeRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public PrizeTypeRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PrizeType>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PrizeType?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }
}
