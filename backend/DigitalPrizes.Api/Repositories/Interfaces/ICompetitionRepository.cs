using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;

namespace DigitalPrizes.Api.Repositories.Interfaces;

/// <summary>
/// Repository interface for Competition entities.
/// </summary>
public interface ICompetitionRepository : IRepository<Competition, int>
{
    /// <summary>
    /// Gets competitions with paging and filtering.
    /// </summary>
    /// <param name="parameters">Paging and filter parameters.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged response of competitions.</returns>
    Task<PagedResponse<Competition>> GetPagedAsync(
        FilterParameters parameters,
        string? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a competition with its registration fields.
    /// </summary>
    /// <param name="competitionId">The competition ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Competition with fields if found.</returns>
    Task<Competition?> GetWithFieldsAsync(int competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a competition with all related data.
    /// </summary>
    /// <param name="competitionId">The competition ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Competition with all relations if found.</returns>
    Task<Competition?> GetWithAllRelationsAsync(int competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets currently active competitions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active competitions.</returns>
    Task<IReadOnlyList<Competition>> GetActiveCompetitionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a registration field by ID.
    /// </summary>
    /// <param name="registrationFieldId">The registration field ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Registration field if found.</returns>
    Task<RegistrationField?> GetFieldByIdAsync(int registrationFieldId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a registration field.
    /// </summary>
    /// <param name="field">The registration field to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    Task UpdateFieldAsync(RegistrationField field, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a registration field.
    /// </summary>
    /// <param name="field">The registration field to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    Task DeleteFieldAsync(RegistrationField field, CancellationToken cancellationToken = default);
}
