using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Competitions;
using DigitalPrizes.Api.Models.Dtos.Registrations;
using DigitalPrizes.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalPrizes.Api.Controllers;

/// <summary>
/// Controller for competition management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class CompetitionsController : ControllerBase
{
    private readonly ICompetitionService _competitionService;
    private readonly IRegistrationService _registrationService;
    private readonly ILogger<CompetitionsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompetitionsController"/> class.
    /// </summary>
    /// <param name="competitionService">The competition service.</param>
    /// <param name="registrationService">The registration service.</param>
    /// <param name="logger">The logger.</param>
    public CompetitionsController(
        ICompetitionService competitionService,
        IRegistrationService registrationService,
        ILogger<CompetitionsController> logger)
    {
        _competitionService = competitionService;
        _registrationService = registrationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets competitions with paging and filtering.
    /// </summary>
    /// <param name="parameters">Paging and filter parameters.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of competitions.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CompetitionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<CompetitionResponse>>> GetCompetitionsAsync(
        [FromQuery] FilterParameters parameters,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting competitions with status filter: {Status}", status);
        var result = await _competitionService.GetCompetitionsAsync(parameters, status, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets active competitions for public listing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active competitions.</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IReadOnlyList<CompetitionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CompetitionResponse>>> GetActiveCompetitionsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting active competitions");
        var competitions = await _competitionService.GetActiveCompetitionsAsync(cancellationToken);
        return Ok(competitions);
    }

    /// <summary>
    /// Gets a competition by its identifier.
    /// </summary>
    /// <param name="id">The competition identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The competition details if found.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CompetitionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompetitionDetailResponse>> GetCompetitionAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting competition {CompetitionId}", id);
        var competition = await _competitionService.GetCompetitionAsync(id, cancellationToken);

        if (competition is null)
        {
            return NotFound();
        }

        return Ok(competition);
    }

    /// <summary>
    /// Creates a new competition.
    /// </summary>
    /// <param name="request">The competition creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created competition.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CompetitionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompetitionResponse>> CreateCompetitionAsync(
        [FromBody] CreateCompetitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var subjectId = User?.Identity?.Name;
        var competition = await _competitionService.CreateCompetitionAsync(request, subjectId, cancellationToken);
        return CreatedAtAction(nameof(GetCompetitionAsync), new { id = competition.CompetitionId }, competition);
    }

    /// <summary>
    /// Updates an existing competition.
    /// </summary>
    /// <param name="id">The competition identifier.</param>
    /// <param name="request">The competition update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated competition.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CompetitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompetitionResponse>> UpdateCompetitionAsync(
        int id,
        [FromBody] UpdateCompetitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var subjectId = User?.Identity?.Name;
        var competition = await _competitionService.UpdateCompetitionAsync(id, request, subjectId, cancellationToken);

        if (competition is null)
        {
            return NotFound();
        }

        return Ok(competition);
    }

    /// <summary>
    /// Updates competition status.
    /// </summary>
    /// <param name="id">The competition identifier.</param>
    /// <param name="newStatus">The new status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatusAsync(
        int id,
        [FromBody] string newStatus,
        CancellationToken cancellationToken = default)
    {
        var subjectId = User?.Identity?.Name;
        var success = await _competitionService.UpdateStatusAsync(id, newStatus, subjectId, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Adds a registration field to a competition.
    /// </summary>
    /// <param name="id">The competition identifier.</param>
    /// <param name="request">The registration field request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created registration field.</returns>
    [HttpPost("{id:int}/fields")]
    [ProducesResponseType(typeof(RegistrationFieldResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegistrationFieldResponse>> AddRegistrationFieldAsync(
        int id,
        [FromBody] CreateRegistrationFieldRequest request,
        CancellationToken cancellationToken = default)
    {
        var field = await _competitionService.AddRegistrationFieldAsync(id, request, cancellationToken);

        if (field is null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(GetCompetitionAsync), new { id }, field);
    }

    /// <summary>
    /// Updates a registration field.
    /// </summary>
    /// <param name="id">The competition identifier.</param>
    /// <param name="fieldId">The registration field identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated registration field.</returns>
    [HttpPut("{id:int}/fields/{fieldId:int}")]
    [ProducesResponseType(typeof(RegistrationFieldResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegistrationFieldResponse>> UpdateRegistrationFieldAsync(
        int id,
        int fieldId,
        [FromBody] UpdateRegistrationFieldRequest request,
        CancellationToken cancellationToken = default)
    {
        var field = await _competitionService.UpdateRegistrationFieldAsync(fieldId, request, cancellationToken);

        if (field is null)
        {
            return NotFound();
        }

        return Ok(field);
    }

    /// <summary>
    /// Deletes a registration field.
    /// </summary>
    /// <param name="id">The competition identifier.</param>
    /// <param name="fieldId">The registration field identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id:int}/fields/{fieldId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRegistrationFieldAsync(
        int id,
        int fieldId,
        CancellationToken cancellationToken = default)
    {
        var success = await _competitionService.DeleteRegistrationFieldAsync(fieldId, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Gets registrations for a competition.
    /// </summary>
    /// <param name="id">The competition identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of registrations.</returns>
    [HttpGet("{id:int}/registrations")]
    [ProducesResponseType(typeof(IReadOnlyList<RegistrationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RegistrationResponse>>> GetRegistrationsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting registrations for competition {CompetitionId}", id);
        var registrations = await _registrationService.GetCompetitionRegistrationsAsync(id, cancellationToken);
        return Ok(registrations);
    }

    /// <summary>
    /// Bulk imports registrations for a competition.
    /// </summary>
    /// <param name="id">The competition identifier.</param>
    /// <param name="registrations">The registrations to import.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Bulk import result.</returns>
    [HttpPost("{id:int}/registrations/bulk")]
    [ProducesResponseType(typeof(BulkRegistrationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<BulkRegistrationResult>> BulkRegisterAsync(
        int id,
        [FromBody] IEnumerable<AdminRegistrationRequest> registrations,
        CancellationToken cancellationToken = default)
    {
        var subjectId = User?.Identity?.Name;
        var result = await _registrationService.BulkRegisterAsync(id, registrations, subjectId, cancellationToken);
        return Ok(result);
    }
}
