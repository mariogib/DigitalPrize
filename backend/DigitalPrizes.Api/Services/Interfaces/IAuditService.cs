namespace DigitalPrizes.Api.Services.Interfaces;

/// <summary>
/// Service interface for audit logging.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an action.
    /// </summary>
    /// <param name="action">The action performed.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="subjectId">The subject performing the action.</param>
    /// <param name="details">Optional details.</param>
    /// <param name="oldValues">Optional old values JSON.</param>
    /// <param name="newValues">Optional new values JSON.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    Task LogAsync(
        string action,
        string entityType,
        string entityId,
        string? subjectId = null,
        string? details = null,
        string? oldValues = null,
        string? newValues = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a competition action.
    /// </summary>
    /// <param name="action">The action performed.</param>
    /// <param name="competitionId">The competition ID.</param>
    /// <param name="subjectId">The subject performing the action.</param>
    /// <param name="details">Optional details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    Task LogCompetitionActionAsync(
        string action,
        int competitionId,
        string? subjectId = null,
        string? details = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a prize award action.
    /// </summary>
    /// <param name="action">The action performed.</param>
    /// <param name="prizeAwardId">The prize award ID.</param>
    /// <param name="subjectId">The subject performing the action.</param>
    /// <param name="details">Optional details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    Task LogPrizeAwardActionAsync(
        string action,
        long prizeAwardId,
        string? subjectId = null,
        string? details = null,
        CancellationToken cancellationToken = default);
}
