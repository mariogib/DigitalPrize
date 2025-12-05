using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Competitions;

namespace DigitalPrizes.Api.Services.Interfaces;

/// <summary>
/// Service interface for competition management.
/// </summary>
public interface ICompetitionService
{
    /// <summary>
    /// Gets competitions with paging and filtering.
    /// </summary>
    /// <param name="parameters">Paging and filtering parameters.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of competitions.</returns>
    Task<PagedResponse<CompetitionResponse>> GetCompetitionsAsync(
        FilterParameters parameters,
        string? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a competition by ID.
    /// </summary>
    /// <param name="competitionId">The competition identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Competition details if found.</returns>
    Task<CompetitionDetailResponse?> GetCompetitionAsync(int competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active competitions for public listing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active competitions.</returns>
    Task<IReadOnlyList<CompetitionResponse>> GetActiveCompetitionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new competition.
    /// </summary>
    /// <param name="request">Create competition request.</param>
    /// <param name="subjectId">Subject performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created competition.</returns>
    Task<CompetitionResponse> CreateCompetitionAsync(
        CreateCompetitionRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a competition.
    /// </summary>
    /// <param name="competitionId">The competition identifier.</param>
    /// <param name="request">Update competition request.</param>
    /// <param name="subjectId">Subject performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated competition if found.</returns>
    Task<CompetitionResponse?> UpdateCompetitionAsync(
        int competitionId,
        UpdateCompetitionRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates competition status.
    /// </summary>
    /// <param name="competitionId">The competition identifier.</param>
    /// <param name="newStatus">The new status.</param>
    /// <param name="subjectId">Subject performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if status was updated.</returns>
    Task<bool> UpdateStatusAsync(
        int competitionId,
        string newStatus,
        string? subjectId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a registration field to a competition.
    /// </summary>
    /// <param name="competitionId">The competition identifier.</param>
    /// <param name="request">Create registration field request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created registration field if competition found.</returns>
    Task<RegistrationFieldResponse?> AddRegistrationFieldAsync(
        int competitionId,
        CreateRegistrationFieldRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a registration field.
    /// </summary>
    /// <param name="registrationFieldId">The registration field identifier.</param>
    /// <param name="request">Update registration field request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated registration field if found.</returns>
    Task<RegistrationFieldResponse?> UpdateRegistrationFieldAsync(
        int registrationFieldId,
        UpdateRegistrationFieldRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a registration field.
    /// </summary>
    /// <param name="registrationFieldId">The registration field identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted.</returns>
    Task<bool> DeleteRegistrationFieldAsync(int registrationFieldId, CancellationToken cancellationToken = default);
}
