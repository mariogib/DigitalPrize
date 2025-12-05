using DigitalPrizes.Api.Models.Dtos.Awards;
using DigitalPrizes.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalPrizes.Api.Controllers.Public;

/// <summary>
/// Public API controller for prize redemption operations.
/// </summary>
[ApiController]
[Route("api/public/redemptions")]
[AllowAnonymous]
public class PublicRedemptionController : ControllerBase
{
    private readonly IRedemptionService _redemptionService;
    private readonly ILogger<PublicRedemptionController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicRedemptionController"/> class.
    /// </summary>
    /// <param name="redemptionService">The redemption service.</param>
    /// <param name="logger">The logger.</param>
    public PublicRedemptionController(
        IRedemptionService redemptionService,
        ILogger<PublicRedemptionController> logger)
    {
        _redemptionService = redemptionService ?? throw new ArgumentNullException(nameof(redemptionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets available prizes for a cell number.
    /// </summary>
    /// <param name="cellNumber">The winner's cell number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of available prizes for redemption.</returns>
    [HttpGet("available")]
    [ProducesResponseType(typeof(IEnumerable<RedeemablePrizeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<RedeemablePrizeResponse>>> GetAvailablePrizesAsync(
        [FromQuery] string cellNumber,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cellNumber))
        {
            return BadRequest(new { message = "Cell number is required." });
        }

        _logger.LogInformation("Getting available prizes for cell {CellNumber}", cellNumber);

        var prizes = await _redemptionService.GetRedeemablePrizesAsync(cellNumber, cancellationToken);

        return Ok(prizes);
    }

    /// <summary>
    /// Initiates prize redemption. Sends OTP to the winner's cell phone.
    /// </summary>
    /// <param name="request">The redemption initiation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redemption initiation result with OTP sent confirmation.</returns>
    [HttpPost("initiate")]
    [ProducesResponseType(typeof(InitiateRedemptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InitiateRedemptionResponse>> InitiateRedemptionAsync(
        [FromBody] InitiateRedemptionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Redemption initiation for award {AwardId} by cell {CellNumber}",
            request.PrizeAwardId,
            request.CellNumber);

        try
        {
            var result = await _redemptionService.InitiateRedemptionAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Redemption initiation failed for award {AwardId}", request.PrizeAwardId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Completes prize redemption by verifying OTP.
    /// </summary>
    /// <param name="request">The redemption completion request with OTP.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redemption completion result.</returns>
    [HttpPost("complete")]
    [ProducesResponseType(typeof(CompleteRedemptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompleteRedemptionResponse>> CompleteRedemptionAsync(
        [FromBody] CompleteRedemptionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Redemption completion for award {PrizeAwardId}",
            request.PrizeAwardId);

        try
        {
            var result = await _redemptionService.CompleteRedemptionAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Redemption completion failed for award {PrizeAwardId}", request.PrizeAwardId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets the redemption details for an award.
    /// </summary>
    /// <param name="prizeAwardId">The prize award ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redemption details.</returns>
    [HttpGet("{prizeAwardId:long}")]
    [ProducesResponseType(typeof(PrizeRedemptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrizeRedemptionResponse>> GetRedemptionAsync(
        long prizeAwardId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting redemption for award {PrizeAwardId}", prizeAwardId);

        var redemption = await _redemptionService.GetRedemptionAsync(prizeAwardId, cancellationToken);

        if (redemption == null)
        {
            return NotFound(new { message = "Redemption not found for this award." });
        }

        return Ok(redemption);
    }
}
