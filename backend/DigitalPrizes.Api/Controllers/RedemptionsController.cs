using DigitalPrizes.Api.Models.Dtos.Awards;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalPrizes.Api.Controllers;

/// <summary>
/// Controller for managing prize redemptions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RedemptionsController : ControllerBase
{
    private readonly IRedemptionService _redemptionService;
    private readonly ILogger<RedemptionsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedemptionsController"/> class.
    /// </summary>
    /// <param name="redemptionService">The redemption service.</param>
    /// <param name="logger">The logger.</param>
    public RedemptionsController(
        IRedemptionService redemptionService,
        ILogger<RedemptionsController> logger)
    {
        _redemptionService = redemptionService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paged list of redemptions.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="sortBy">Sort field.</param>
    /// <param name="sortDescending">Sort descending.</param>
    /// <param name="searchTerm">Search term.</param>
    /// <param name="competitionId">Optional competition filter.</param>
    /// <param name="prizePoolId">Optional prize pool filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of redemptions.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<PrizeRedemptionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<PrizeRedemptionResponse>>> GetRedemptionsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? competitionId = null,
        [FromQuery] int? prizePoolId = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new FilterParameters
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending,
            SearchTerm = searchTerm,
        };

        var result = await _redemptionService.GetRedemptionsAsync(
            parameters,
            competitionId,
            prizePoolId,
            cancellationToken);

        return Ok(result);
    }
}
