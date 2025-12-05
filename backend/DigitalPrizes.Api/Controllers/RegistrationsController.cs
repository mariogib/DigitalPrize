using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Registrations;
using DigitalPrizes.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalPrizes.Api.Controllers;

/// <summary>
/// Controller for registration management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    private readonly ILogger<RegistrationsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationsController"/> class.
    /// </summary>
    /// <param name="registrationService">The registration service.</param>
    /// <param name="logger">The logger.</param>
    public RegistrationsController(
        IRegistrationService registrationService,
        ILogger<RegistrationsController> logger)
    {
        _registrationService = registrationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets registrations with paging and filtering.
    /// </summary>
    /// <param name="parameters">Paging and filter parameters.</param>
    /// <param name="competitionId">Optional competition filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of registrations.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<RegistrationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<RegistrationResponse>>> GetRegistrationsAsync(
        [FromQuery] FilterParameters parameters,
        [FromQuery] int? competitionId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting registrations with filters");
        var result = await _registrationService.GetRegistrationsAsync(parameters, competitionId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a registration by its identifier.
    /// </summary>
    /// <param name="id">The registration identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The registration details if found.</returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegistrationResponse>> GetRegistrationAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting registration {RegistrationId}", id);
        var registration = await _registrationService.GetRegistrationAsync(id, cancellationToken);

        if (registration is null)
        {
            return NotFound();
        }

        return Ok(registration);
    }
}
