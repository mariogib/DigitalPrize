using DigitalPrizes.Api.Models.Domain;

namespace DigitalPrizes.Api.Repositories.Interfaces;

/// <summary>
/// Repository interface for PrizeType entities.
/// </summary>
public interface IPrizeTypeRepository : IRepository<PrizeType, int>
{
    /// <summary>
    /// Gets active prize types.
    /// </summary>
    Task<IReadOnlyList<PrizeType>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a prize type by name.
    /// </summary>
    Task<PrizeType?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
