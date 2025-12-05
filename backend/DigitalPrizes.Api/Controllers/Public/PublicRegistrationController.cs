using DigitalPrizes.Api.Models.Dtos.Registrations;
using DigitalPrizes.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalPrizes.Api.Controllers.Public;

/// <summary>
/// Public API controller for user registration operations.
/// </summary>
[ApiController]
[Route("api/public/registrations")]
[AllowAnonymous]
public class PublicRegistrationController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    private readonly ICompetitionService _competitionService;
    private readonly ILogger<PublicRegistrationController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicRegistrationController"/> class.
    /// </summary>
    /// <param name="registrationService">The registration service.</param>
    /// <param name="competitionService">The competition service.</param>
    /// <param name="logger">The logger.</param>
    public PublicRegistrationController(
        IRegistrationService registrationService,
        ICompetitionService competitionService,
        ILogger<PublicRegistrationController> logger)
    {
        _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
        _competitionService = competitionService ?? throw new ArgumentNullException(nameof(competitionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a user for a competition. Sends OTP for verification.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Registration result with OTP sent confirmation.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PublicRegistrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicRegistrationResponse>> RegisterAsync(
        [FromBody] PublicRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Public registration request for competition {CompetitionId} with cell {CellNumber}",
            request.CompetitionId,
            request.CellNumber);

        // Validate competition exists and is active
        var competition = await _competitionService.GetCompetitionAsync(request.CompetitionId, cancellationToken);
        if (competition == null)
        {
            return NotFound(new { message = $"Competition with ID {request.CompetitionId} not found." });
        }

        if (!string.Equals(competition.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Competition is not currently accepting registrations." });
        }

        try
        {
            var result = await _registrationService.RegisterAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Registration failed for cell {CellNumber}", request.CellNumber);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Verifies a registration using the OTP sent to the user's cell phone.
    /// </summary>
    /// <param name="request">The verification request with OTP.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Verification result.</returns>
    [HttpPost("verify")]
    [ProducesResponseType(typeof(PublicRegistrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PublicRegistrationResponse>> VerifyRegistrationAsync(
        [FromBody] VerifyRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Verification request for registration with OTP");

        try
        {
            // First get registration to get cell number and competition ID
            var registration = await _registrationService.GetRegistrationAsync(request.RegistrationId, cancellationToken);
            if (registration == null)
            {
                return NotFound(new { message = "Registration not found." });
            }

            var result = await _registrationService.VerifyAndCompleteRegistrationAsync(
                registration.CellNumber,
                request.Otp,
                registration.CompetitionId,
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Verification failed for registration {RegistrationId}", request.RegistrationId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets the registration status for a user in a competition.
    /// </summary>
    /// <param name="competitionId">The competition ID.</param>
    /// <param name="cellNumber">The user's cell number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Whether the user is registered.</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRegistrationStatusAsync(
        [FromQuery] int competitionId,
        [FromQuery] string cellNumber,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cellNumber))
        {
            return BadRequest(new { message = "Cell number is required." });
        }

        _logger.LogInformation(
            "Registration status request for competition {CompetitionId} and cell {CellNumber}",
            competitionId,
            cellNumber);

        var isRegistered = await _registrationService.IsRegisteredAsync(competitionId, cellNumber, cancellationToken);

        return Ok(new
        {
            competitionId,
            cellNumber,
            isRegistered,
        });
    }
}
