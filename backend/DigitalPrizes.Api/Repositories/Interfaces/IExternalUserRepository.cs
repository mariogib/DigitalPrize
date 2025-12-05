using DigitalPrizes.Api.Models.Domain;

namespace DigitalPrizes.Api.Repositories.Interfaces;

/// <summary>
/// Repository interface for ExternalUser entities.
/// </summary>
public interface IExternalUserRepository : IRepository<ExternalUser, long>
{
    /// <summary>
    /// Gets an external user by cell number.
    /// </summary>
    Task<ExternalUser?> GetByCellNumberAsync(string cellNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an external user with all related data.
    /// </summary>
    Task<ExternalUser?> GetWithAllRelationsAsync(long externalUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or creates an external user by cell number.
    /// </summary>
    Task<ExternalUser> GetOrCreateAsync(
        string cellNumber,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        CancellationToken cancellationToken = default);
}
