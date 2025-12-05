using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Prizes;
using DigitalPrizes.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalPrizes.Api.Controllers;

/// <summary>
/// Controller for prize management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class PrizesController : ControllerBase
{
    private readonly IPrizeService _prizeService;
    private readonly ILogger<PrizesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrizesController"/> class.
    /// </summary>
    /// <param name="prizeService">The prize service.</param>
    /// <param name="logger">The logger.</param>
    public PrizesController(
        IPrizeService prizeService,
        ILogger<PrizesController> logger)
    {
        _prizeService = prizeService;
        _logger = logger;
    }

    /// <summary>
    /// Gets prizes with paging and filtering.
    /// </summary>
    /// <param name="parameters">Paging and filter parameters.</param>
    /// <param name="prizePoolId">Optional prize pool filter.</param>
    /// <param name="prizeTypeId">Optional prize type filter.</param>
    /// <param name="isActive">Optional active status filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of prizes.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResponse<PrizeSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<PrizeSummaryResponse>>> GetPrizesAsync(
        [FromQuery] FilterParameters parameters,
        [FromQuery] int? prizePoolId = null,
        [FromQuery] int? prizeTypeId = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting prizes with filters");
        var result = await _prizeService.GetPrizesAsync(parameters, prizePoolId, prizeTypeId, isActive, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a prize by its identifier.
    /// </summary>
    /// <param name="id">The prize identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The prize details if found.</returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(PrizeDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrizeDetailResponse>> GetPrizeAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting prize {PrizeId}", id);
        var prize = await _prizeService.GetPrizeAsync(id, cancellationToken);

        if (prize is null)
        {
            return NotFound();
        }

        return Ok(prize);
    }

    /// <summary>
    /// Gets all prize types.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of prize types.</returns>
    [HttpGet("types")]
    [ProducesResponseType(typeof(IReadOnlyList<PrizeTypeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PrizeTypeResponse>>> GetPrizeTypesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting prize types");
        var types = await _prizeService.GetPrizeTypesAsync(cancellationToken);
        return Ok(types);
    }

    /// <summary>
    /// Creates a new prize.
    /// </summary>
    /// <param name="request">The prize creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created prize.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PrizeSummaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrizeSummaryResponse>> CreatePrizeAsync(
        [FromBody] CreatePrizeRequest request,
        CancellationToken cancellationToken = default)
    {
        var subjectId = User?.Identity?.Name;
        var prize = await _prizeService.CreatePrizeAsync(request, subjectId, cancellationToken);
        return CreatedAtAction("GetPrize", new { id = prize.PrizeId }, prize);
    }

    /// <summary>
    /// Bulk creates prizes.
    /// </summary>
    /// <param name="request">The bulk creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of created prizes.</returns>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(IReadOnlyList<PrizeSummaryResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PrizeSummaryResponse>>> BulkCreatePrizesAsync(
        [FromBody] BulkCreatePrizesRequest request,
        CancellationToken cancellationToken = default)
    {
        var subjectId = User?.Identity?.Name;
        var prizes = await _prizeService.BulkCreatePrizesAsync(request, subjectId, cancellationToken);
        return CreatedAtAction("GetPrizes", prizes);
    }

    /// <summary>
    /// Updates an existing prize.
    /// </summary>
    /// <param name="id">The prize identifier.</param>
    /// <param name="request">The prize update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated prize.</returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(PrizeSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrizeSummaryResponse>> UpdatePrizeAsync(
        long id,
        [FromBody] UpdatePrizeRequest request,
        CancellationToken cancellationToken = default)
    {
        var subjectId = User?.Identity?.Name;
        var prize = await _prizeService.UpdatePrizeAsync(id, request, subjectId, cancellationToken);

        if (prize is null)
        {
            return NotFound();
        }

        return Ok(prize);
    }

    /// <summary>
    /// Deletes a prize.
    /// </summary>
    /// <param name="id">The prize identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePrizeAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var subjectId = User?.Identity?.Name;
        var success = await _prizeService.DeletePrizeAsync(id, subjectId, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
