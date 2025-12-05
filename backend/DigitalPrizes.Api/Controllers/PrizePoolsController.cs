using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Prizes;
using DigitalPrizes.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalPrizes.Api.Controllers;

/// <summary>
/// Controller for prize pool management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class PrizePoolsController : ControllerBase
{
    private readonly IPrizePoolService _prizePoolService;
    private readonly ILogger<PrizePoolsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrizePoolsController"/> class.
    /// </summary>
    /// <param name="prizePoolService">The prize pool service.</param>
    /// <param name="logger">The logger.</param>
    public PrizePoolsController(
        IPrizePoolService prizePoolService,
        ILogger<PrizePoolsController> logger)
    {
        _prizePoolService = prizePoolService;
        _logger = logger;
    }

    /// <summary>
    /// Gets prize pools with paging and filtering.
    /// </summary>
    /// <param name="parameters">Paging and filter parameters.</param>
    /// <param name="competitionId">Optional competition filter.</param>
    /// <param name="isActive">Optional active status filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of prize pools.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<PrizePoolSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<PrizePoolSummaryResponse>>> GetPrizePoolsAsync(
        [FromQuery] FilterParameters parameters,
        [FromQuery] int? competitionId = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting prize pools with filters");
        var result = await _prizePoolService.GetPrizePoolsAsync(parameters, competitionId, isActive, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a prize pool by its identifier.
    /// </summary>
    /// <param name="id">The prize pool identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The prize pool details if found.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PrizePoolDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrizePoolDetailResponse>> GetPrizePoolAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting prize pool {PrizePoolId}", id);
        var prizePool = await _prizePoolService.GetPrizePoolAsync(id, cancellationToken);

        if (prizePool is null)
        {
            return NotFound();
        }

        return Ok(prizePool);
    }

    /// <summary>
    /// Creates a new prize pool.
    /// </summary>
    /// <param name="request">The prize pool creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created prize pool.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PrizePoolSummaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrizePoolSummaryResponse>> CreatePrizePoolAsync(
        [FromBody] CreatePrizePoolRequest request,
        CancellationToken cancellationToken = default)
    {
        var subjectId = User?.Identity?.Name;
        var prizePool = await _prizePoolService.CreatePrizePoolAsync(request, subjectId, cancellationToken);
        return CreatedAtAction("GetPrizePool", new { id = prizePool.PrizePoolId }, prizePool);
    }

    /// <summary>
    /// Updates an existing prize pool.
    /// </summary>
    /// <param name="id">The prize pool identifier.</param>
    /// <param name="request">The prize pool update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated prize pool.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PrizePoolSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrizePoolSummaryResponse>> UpdatePrizePoolAsync(
        int id,
        [FromBody] UpdatePrizePoolRequest request,
        CancellationToken cancellationToken = default)
    {
        var subjectId = User?.Identity?.Name;
        var prizePool = await _prizePoolService.UpdatePrizePoolAsync(id, request, subjectId, cancellationToken);

        if (prizePool is null)
        {
            return NotFound();
        }

        return Ok(prizePool);
    }

    /// <summary>
    /// Deletes a prize pool.
    /// </summary>
    /// <param name="id">The prize pool identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePrizePoolAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var subjectId = User?.Identity?.Name;
        var success = await _prizePoolService.DeletePrizePoolAsync(id, subjectId, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
