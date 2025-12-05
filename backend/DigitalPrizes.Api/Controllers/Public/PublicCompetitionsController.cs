using DigitalPrizes.Api.Models.Dtos.Competitions;
using DigitalPrizes.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalPrizes.Api.Controllers.Public;

/// <summary>
/// Public API controller for retrieving active competitions.
/// </summary>
[ApiController]
[Route("api/public/competitions")]
[AllowAnonymous]
public class PublicCompetitionsController : ControllerBase
{
    private readonly ICompetitionService _competitionService;
    private readonly ILogger<PublicCompetitionsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicCompetitionsController"/> class.
    /// </summary>
    /// <param name="competitionService">The competition service.</param>
    /// <param name="logger">The logger.</param>
    public PublicCompetitionsController(
        ICompetitionService competitionService,
        ILogger<PublicCompetitionsController> logger)
    {
        _competitionService = competitionService ?? throw new ArgumentNullException(nameof(competitionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all active competitions available for registration.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active competitions.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CompetitionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CompetitionResponse>>> GetActiveCompetitionsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Public request to get active competitions");

        var competitions = await _competitionService.GetActiveCompetitionsAsync(cancellationToken);

        return Ok(competitions);
    }

    /// <summary>
    /// Gets details of a specific active competition.
    /// </summary>
    /// <param name="id">The competition ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Competition details if found and active.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CompetitionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompetitionDetailResponse>> GetCompetitionByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Public request to get competition {CompetitionId}", id);

        var competition = await _competitionService.GetCompetitionAsync(id, cancellationToken);

        if (competition == null)
        {
            return NotFound(new { message = $"Competition with ID {id} not found." });
        }

        // Only return if competition is active
        if (!string.Equals(competition.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { message = "Competition is not currently active." });
        }

        return Ok(competition);
    }
}
