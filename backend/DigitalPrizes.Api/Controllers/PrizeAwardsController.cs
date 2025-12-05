using DigitalPrizes.Api.Models.Dtos.Awards;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Reports;
using DigitalPrizes.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalPrizes.Api.Controllers;

/// <summary>
/// Controller for prize award management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class PrizeAwardsController : ControllerBase
{
    private readonly IPrizeAwardService _prizeAwardService;
    private readonly ILogger<PrizeAwardsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrizeAwardsController"/> class.
    /// </summary>
    /// <param name="prizeAwardService">The prize award service.</param>
    /// <param name="logger">The logger.</param>
    public PrizeAwardsController(
        IPrizeAwardService prizeAwardService,
        ILogger<PrizeAwardsController> logger)
    {
        _prizeAwardService = prizeAwardService;
        _logger = logger;
    }

    /// <summary>
    /// Gets prize awards with paging and filtering.
    /// </summary>
    /// <param name="parameters">Paging and filter parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of prize awards.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<PrizeAwardResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<PrizeAwardResponse>>> GetAwardsAsync(
        [FromQuery] AwardsReportParameters parameters,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting prize awards with filters");
        var result = await _prizeAwardService.GetAwardsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a prize award by its identifier.
    /// </summary>
    /// <param name="id">The prize award identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The prize award details if found.</returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(PrizeAwardDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrizeAwardDetailResponse>> GetAwardAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting prize award {PrizeAwardId}", id);
        var award = await _prizeAwardService.GetAwardAsync(id, cancellationToken);

        if (award is null)
        {
            return NotFound();
        }

        return Ok(award);
    }

    /// <summary>
    /// Gets awards for a cell number.
    /// </summary>
    /// <param name="cellNumber">The cell number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of awards for the cell number.</returns>
    [HttpGet("by-cell/{cellNumber}")]
    [ProducesResponseType(typeof(IReadOnlyList<PrizeAwardResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PrizeAwardResponse>>> GetAwardsByCellNumberAsync(
        string cellNumber,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting awards for cell number {CellNumber}", cellNumber);
        var awards = await _prizeAwardService.GetAwardsByCellNumberAsync(cellNumber, cancellationToken);
        return Ok(awards);
    }

    /// <summary>
    /// Awards a prize to a user.
    /// </summary>
    /// <param name="request">The award prize request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created prize award.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PrizeAwardResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrizeAwardResponse>> AwardPrizeAsync(
        [FromBody] AwardPrizeRequest request,
        CancellationToken cancellationToken = default)
    {
        var subjectId = User?.Identity?.Name;
        var award = await _prizeAwardService.AwardPrizeAsync(request, subjectId, cancellationToken);

        if (award is null)
        {
            return BadRequest("Unable to award prize. Prize may not be available.");
        }

        return CreatedAtAction(nameof(GetAwardAsync), new { id = award.PrizeAwardId }, award);
    }

    /// <summary>
    /// Bulk awards prizes to multiple users.
    /// </summary>
    /// <param name="request">The bulk award request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Bulk award result.</returns>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(BulkAwardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BulkAwardResponse>> BulkAwardPrizesAsync(
        [FromBody] BulkAwardPrizesRequest request,
        CancellationToken cancellationToken = default)
    {
        var subjectId = User?.Identity?.Name;
        var result = await _prizeAwardService.BulkAwardPrizesAsync(request, subjectId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cancels a prize award.
    /// </summary>
    /// <param name="id">The prize award identifier.</param>
    /// <param name="request">The cancellation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPost("{id:long}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelAwardAsync(
        long id,
        [FromBody] CancelAwardRequest request,
        CancellationToken cancellationToken = default)
    {
        var subjectId = User?.Identity?.Name;
        var success = await _prizeAwardService.CancelAwardAsync(id, request, subjectId, cancellationToken);

        if (!success)
        {
            return BadRequest("Unable to cancel award. Award may not exist or may not be in a cancellable state.");
        }

        return NoContent();
    }

    /// <summary>
    /// Resends notification for an award.
    /// </summary>
    /// <param name="id">The prize award identifier.</param>
    /// <param name="notificationChannel">Optional notification channel override.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPost("{id:long}/resend-notification")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResendNotificationAsync(
        long id,
        [FromQuery] string? notificationChannel = null,
        CancellationToken cancellationToken = default)
    {
        var success = await _prizeAwardService.ResendNotificationAsync(id, notificationChannel, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
