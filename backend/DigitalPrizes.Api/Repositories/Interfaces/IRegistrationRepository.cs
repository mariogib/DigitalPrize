using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;

namespace DigitalPrizes.Api.Repositories.Interfaces;

/// <summary>
/// Repository interface for Registration entities.
/// </summary>
public interface IRegistrationRepository : IRepository<Registration, long>
{
    /// <summary>
    /// Gets registrations with paging and filtering.
    /// </summary>
    Task<PagedResponse<Registration>> GetPagedAsync(
        FilterParameters parameters,
        int? competitionId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a registration by competition and cell number.
    /// </summary>
    Task<Registration?> GetByCompetitionAndCellAsync(
        int competitionId,
        string cellNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a registration with all related data.
    /// </summary>
    Task<Registration?> GetWithAnswersAsync(long registrationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets registrations for a competition.
    /// </summary>
    Task<IReadOnlyList<Registration>> GetByCompetitionAsync(
        int competitionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets registrations for an external user.
    /// </summary>
    Task<IReadOnlyList<Registration>> GetByExternalUserAsync(
        long externalUserId,
        CancellationToken cancellationToken = default);
}
