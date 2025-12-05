using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Registrations;

namespace DigitalPrizes.Api.Services.Interfaces;

/// <summary>
/// Service interface for competition registration.
/// </summary>
public interface IRegistrationService
{
    /// <summary>
    /// Gets registrations with paging and filtering.
    /// </summary>
    /// <param name="parameters">Filter parameters.</param>
    /// <param name="competitionId">Optional competition filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of registrations.</returns>
    Task<PagedResponse<RegistrationResponse>> GetRegistrationsAsync(
        FilterParameters parameters,
        int? competitionId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a user for a competition (public endpoint).
    /// </summary>
    /// <param name="request">Registration request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Registration response.</returns>
    Task<PublicRegistrationResponse> RegisterAsync(
        PublicRegistrationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies OTP and completes registration.
    /// </summary>
    /// <param name="cellNumber">Cell number to verify.</param>
    /// <param name="otpCode">OTP code.</param>
    /// <param name="competitionId">Competition identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Registration response.</returns>
    Task<PublicRegistrationResponse> VerifyAndCompleteRegistrationAsync(
        string cellNumber,
        string otpCode,
        int competitionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a registration by ID.
    /// </summary>
    /// <param name="registrationId">Registration identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Registration if found.</returns>
    Task<RegistrationResponse?> GetRegistrationAsync(long registrationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets registrations for a competition.
    /// </summary>
    /// <param name="competitionId">Competition identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of registrations.</returns>
    Task<IReadOnlyList<RegistrationResponse>> GetCompetitionRegistrationsAsync(
        int competitionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a cell number is registered for a competition.
    /// </summary>
    /// <param name="competitionId">Competition identifier.</param>
    /// <param name="cellNumber">Cell number to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if registered.</returns>
    Task<bool> IsRegisteredAsync(int competitionId, string cellNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin bulk registration import.
    /// </summary>
    /// <param name="competitionId">Competition identifier.</param>
    /// <param name="registrations">List of registrations.</param>
    /// <param name="subjectId">Subject performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Bulk registration result.</returns>
    Task<BulkRegistrationResult> BulkRegisterAsync(
        int competitionId,
        IEnumerable<AdminRegistrationRequest> registrations,
        string? subjectId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of bulk registration operation.
/// </summary>
public record BulkRegistrationResult
{
    public int TotalRequested { get; init; }
    public int SuccessfulRegistrations { get; init; }
    public int FailedRegistrations { get; init; }
    public int DuplicateRegistrations { get; init; }
    public List<BulkRegistrationItemResult> Results { get; init; } = new();
}

/// <summary>
/// Result for individual bulk registration item.
/// </summary>
public record BulkRegistrationItemResult
{
    public string CellNumber { get; init; } = string.Empty;
    public bool Success { get; init; }
    public long? RegistrationId { get; init; }
    public string? ErrorMessage { get; init; }
}
