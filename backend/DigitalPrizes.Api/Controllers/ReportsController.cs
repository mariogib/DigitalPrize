using DigitalPrizes.Api.Models.Dtos.Reports;
using DigitalPrizes.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalPrizes.Api.Controllers;

/// <summary>
/// Controller for reports and analytics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportsController"/> class.
    /// </summary>
    /// <param name="reportService">The report service.</param>
    /// <param name="logger">The logger.</param>
    public ReportsController(
        IReportService reportService,
        ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// Gets dashboard summary statistics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dashboard summary.</returns>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardSummary), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummary>> GetDashboardSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting dashboard summary");
        var summary = await _reportService.GetDashboardSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Gets registration statistics.
    /// </summary>
    /// <param name="competitionId">Optional competition filter.</param>
    /// <param name="fromDate">Optional start date.</param>
    /// <param name="toDate">Optional end date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Registration statistics.</returns>
    [HttpGet("registrations")]
    [ProducesResponseType(typeof(RegistrationStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<RegistrationStats>> GetRegistrationStatsAsync(
        [FromQuery] int? competitionId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting registration stats");
        var stats = await _reportService.GetRegistrationStatsAsync(competitionId, fromDate, toDate, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Gets prize award statistics.
    /// </summary>
    /// <param name="competitionId">Optional competition filter.</param>
    /// <param name="prizePoolId">Optional prize pool filter.</param>
    /// <param name="fromDate">Optional start date.</param>
    /// <param name="toDate">Optional end date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Award statistics.</returns>
    [HttpGet("awards")]
    [ProducesResponseType(typeof(AwardStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<AwardStats>> GetAwardStatsAsync(
        [FromQuery] int? competitionId = null,
        [FromQuery] int? prizePoolId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting award stats");
        var stats = await _reportService.GetAwardStatsAsync(competitionId, prizePoolId, fromDate, toDate, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Gets redemption statistics.
    /// </summary>
    /// <param name="competitionId">Optional competition filter.</param>
    /// <param name="fromDate">Optional start date.</param>
    /// <param name="toDate">Optional end date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redemption statistics.</returns>
    [HttpGet("redemptions")]
    [ProducesResponseType(typeof(RedemptionStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<RedemptionStats>> GetRedemptionStatsAsync(
        [FromQuery] int? competitionId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting redemption stats");
        var stats = await _reportService.GetRedemptionStatsAsync(competitionId, fromDate, toDate, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Exports awards report.
    /// </summary>
    /// <param name="request">Export request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>File download.</returns>
    [HttpPost("export/awards")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportAwardsReportAsync(
        [FromBody] ExportReportRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting awards report");
        var bytes = await _reportService.ExportAwardsReportAsync(request, cancellationToken);

        var contentType = string.Equals(request.Format, "CSV", StringComparison.OrdinalIgnoreCase)
            ? "text/csv"
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        var extension = string.Equals(request.Format, "CSV", StringComparison.OrdinalIgnoreCase)
            ? "csv"
            : "xlsx";

        return File(bytes, contentType, $"awards-report-{DateTime.UtcNow:yyyyMMdd}.{extension}");
    }

    /// <summary>
    /// Exports registrations report.
    /// </summary>
    /// <param name="request">Export request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>File download.</returns>
    [HttpPost("export/registrations")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportRegistrationsReportAsync(
        [FromBody] ExportReportRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting registrations report");
        var bytes = await _reportService.ExportRegistrationsReportAsync(request, cancellationToken);

        var contentType = string.Equals(request.Format, "CSV", StringComparison.OrdinalIgnoreCase)
            ? "text/csv"
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        var extension = string.Equals(request.Format, "CSV", StringComparison.OrdinalIgnoreCase)
            ? "csv"
            : "xlsx";

        return File(bytes, contentType, $"registrations-report-{DateTime.UtcNow:yyyyMMdd}.{extension}");
    }
}
