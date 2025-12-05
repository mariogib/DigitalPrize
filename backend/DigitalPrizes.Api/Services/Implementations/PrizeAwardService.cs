using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Awards;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Reports;
using DigitalPrizes.Api.Repositories.Interfaces;
using DigitalPrizes.Api.Services.Interfaces;

namespace DigitalPrizes.Api.Services.Implementations;

/// <summary>
/// Service implementation for prize award operations.
/// </summary>
public class PrizeAwardService : IPrizeAwardService
{
    private readonly IPrizeAwardRepository _prizeAwardRepository;
    private readonly IPrizeRepository _prizeRepository;
    private readonly IExternalUserRepository _externalUserRepository;
    private readonly ISmsService _smsService;
    private readonly IAuditService _auditService;
    private readonly ILogger<PrizeAwardService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrizeAwardService"/> class.
    /// </summary>
    /// <param name="prizeAwardRepository">The prize award repository.</param>
    /// <param name="prizeRepository">The prize repository.</param>
    /// <param name="externalUserRepository">The external user repository.</param>
    /// <param name="smsService">The SMS service.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="logger">The logger.</param>
    public PrizeAwardService(
        IPrizeAwardRepository prizeAwardRepository,
        IPrizeRepository prizeRepository,
        IExternalUserRepository externalUserRepository,
        ISmsService smsService,
        IAuditService auditService,
        ILogger<PrizeAwardService> logger)
    {
        _prizeAwardRepository = prizeAwardRepository;
        _prizeRepository = prizeRepository;
        _externalUserRepository = externalUserRepository;
        _smsService = smsService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PagedResponse<PrizeAwardResponse>> GetAwardsAsync(
        AwardsReportParameters parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var result = await _prizeAwardRepository.GetPagedAsync(parameters, cancellationToken);

        var items = result.Items.Select(MapToResponse).ToList();

        return new PagedResponse<PrizeAwardResponse>
        {
            Items = items,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
        };
    }

    /// <inheritdoc/>
    public async Task<PrizeAwardDetailResponse?> GetAwardAsync(long prizeAwardId, CancellationToken cancellationToken = default)
    {
        var award = await _prizeAwardRepository.GetWithRelationsAsync(prizeAwardId, cancellationToken);

        if (award is null)
        {
            return null;
        }

        return MapToDetailResponse(award);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PrizeAwardResponse>> GetAwardsByCellNumberAsync(
        string cellNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cellNumber);

        var awards = await _prizeAwardRepository.GetByCellNumberAsync(cellNumber, cancellationToken);
        return awards.Select(MapToResponse).ToList();
    }

    /// <inheritdoc/>
    public async Task<PrizeAwardResponse?> AwardPrizeAsync(
        AwardPrizeRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var prize = await _prizeRepository.GetByIdAsync(request.PrizeId, cancellationToken);

        if (prize is null)
        {
            _logger.LogWarning("Prize {PrizeId} not found", request.PrizeId);
            return null;
        }

        if (prize.RemainingQuantity <= 0)
        {
            _logger.LogWarning("Prize {PrizeId} has no available quantity", request.PrizeId);
            return null;
        }

        // Get or create external user
        var externalUser = await GetOrCreateExternalUserAsync(request.CellNumber, cancellationToken);

        // Calculate expiry date
        DateTime? expiryDate = null;
        if (request.ExpiryDays.HasValue)
        {
            expiryDate = DateTime.UtcNow.AddDays(request.ExpiryDays.Value);
        }
        else if (prize.ExpiryDate.HasValue)
        {
            expiryDate = prize.ExpiryDate;
        }

        var prizeAward = new PrizeAward
        {
            PrizeId = request.PrizeId,
            ExternalUserId = externalUser.ExternalUserId,
            CellNumber = request.CellNumber,
            CompetitionId = request.CompetitionId,
            AwardedAt = DateTime.UtcNow,
            AwardedBySubjectId = subjectId,
            AwardMethod = AwardMethod.Manual,
            Status = AwardStatus.Awarded,
            ExpiryDate = expiryDate,
            NotificationChannel = request.NotificationChannel,
            NotificationStatus = request.SendNotification ? NotificationStatus.Pending : "NotRequired",
            ExternalReference = request.ExternalReference,
        };

        await _prizeAwardRepository.AddAsync(prizeAward, cancellationToken);

        // Decrement available quantity
        await _prizeRepository.DecrementAvailableQuantityAsync(request.PrizeId, cancellationToken);

        // Send notification if requested
        if (request.SendNotification)
        {
            await SendAwardNotificationAsync(prizeAward, prize, cancellationToken);
        }

        await _auditService.LogPrizeAwardActionAsync(
            AuditAction.Award,
            prizeAward.PrizeAwardId,
            subjectId,
            $"Awarded prize {prize.Name} to {request.CellNumber}",
            cancellationToken);

        _logger.LogInformation(
            "Prize {PrizeId} awarded to {CellNumber}, award ID: {PrizeAwardId}",
            request.PrizeId,
            request.CellNumber,
            prizeAward.PrizeAwardId);

        // Reload with relations
        var result = await _prizeAwardRepository.GetWithRelationsAsync(prizeAward.PrizeAwardId, cancellationToken);
        return result is not null ? MapToResponse(result) : null;
    }

    /// <inheritdoc/>
    public async Task<BulkAwardResponse> BulkAwardPrizesAsync(
        BulkAwardPrizesRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var results = new List<BulkAwardItemResult>();
        var successCount = 0;
        var failCount = 0;

        foreach (var cellNumber in request.CellNumbers)
        {
            try
            {
                // Get next available prize
                var prize = await _prizeRepository.GetNextAvailableAsync(
                    request.PrizePoolId,
                    request.PrizeTypeId,
                    cancellationToken);

                if (prize is null)
                {
                    results.Add(new BulkAwardItemResult
                    {
                        CellNumber = cellNumber,
                        Success = false,
                        ErrorMessage = "No available prizes in the pool.",
                    });
                    failCount++;
                    continue;
                }

                var awardRequest = new AwardPrizeRequest
                {
                    PrizeId = prize.PrizeId,
                    CellNumber = cellNumber,
                    CompetitionId = request.CompetitionId,
                    NotificationChannel = request.NotificationChannel,
                    SendNotification = request.SendNotification,
                    ExpiryDays = request.ExpiryDays,
                };

                var award = await AwardPrizeAsync(awardRequest, subjectId, cancellationToken);

                if (award is not null)
                {
                    results.Add(new BulkAwardItemResult
                    {
                        CellNumber = cellNumber,
                        Success = true,
                        PrizeAwardId = award.PrizeAwardId,
                    });
                    successCount++;
                }
                else
                {
                    results.Add(new BulkAwardItemResult
                    {
                        CellNumber = cellNumber,
                        Success = false,
                        ErrorMessage = "Failed to award prize.",
                    });
                    failCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to award prize to {CellNumber}", cellNumber);
                results.Add(new BulkAwardItemResult
                {
                    CellNumber = cellNumber,
                    Success = false,
                    ErrorMessage = ex.Message,
                });
                failCount++;
            }
        }

        await _auditService.LogAsync(
            AuditAction.BulkAward,
            "PrizeAward",
            "Bulk",
            subjectId,
            $"Bulk awarded {successCount} prizes, {failCount} failed",
            cancellationToken: cancellationToken);

        return new BulkAwardResponse
        {
            TotalRequested = request.CellNumbers.Count,
            SuccessfulAwards = successCount,
            FailedAwards = failCount,
            Results = results,
        };
    }

    /// <inheritdoc/>
    public async Task<bool> CancelAwardAsync(
        long prizeAwardId,
        CancelAwardRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var award = await _prizeAwardRepository.GetByIdAsync(prizeAwardId, cancellationToken);

        if (award is null)
        {
            return false;
        }

        if (award.Status != AwardStatus.Awarded)
        {
            _logger.LogWarning("Cannot cancel award {PrizeAwardId} in status: {Status}", prizeAwardId, award.Status);
            return false;
        }

        award.Status = AwardStatus.Cancelled;

        await _prizeAwardRepository.UpdateAsync(award, cancellationToken);

        await _auditService.LogPrizeAwardActionAsync(
            AuditAction.Cancel,
            prizeAwardId,
            subjectId,
            $"Cancelled award: {request.Reason}",
            cancellationToken);

        _logger.LogInformation("Prize award {PrizeAwardId} cancelled by {SubjectId}", prizeAwardId, subjectId);

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> ResendNotificationAsync(
        long prizeAwardId,
        string? notificationChannel = null,
        CancellationToken cancellationToken = default)
    {
        var award = await _prizeAwardRepository.GetWithRelationsAsync(prizeAwardId, cancellationToken);

        if (award?.Prize is null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(notificationChannel))
        {
            award.NotificationChannel = notificationChannel;
        }

        await SendAwardNotificationAsync(award, award.Prize, cancellationToken);

        await _auditService.LogPrizeAwardActionAsync(
            AuditAction.Update,
            prizeAwardId,
            subjectId: null,
            "Resent notification",
            cancellationToken);

        return true;
    }

    private async Task<ExternalUser> GetOrCreateExternalUserAsync(
        string cellNumber,
        CancellationToken cancellationToken)
    {
        var user = await _externalUserRepository.GetByCellNumberAsync(cellNumber, cancellationToken);

        if (user is not null)
        {
            return user;
        }

        user = new ExternalUser
        {
            CellNumber = cellNumber,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };

        await _externalUserRepository.AddAsync(user, cancellationToken);
        return user;
    }

    private async Task SendAwardNotificationAsync(PrizeAward award, Prize prize, CancellationToken cancellationToken)
    {
        var smsResult = await _smsService.SendPrizeNotificationAsync(
            award.CellNumber,
            prize.Name,
            expiryDate: award.ExpiryDate,
            cancellationToken: cancellationToken);

        award.NotificationStatus = smsResult.Success ? NotificationStatus.Sent : NotificationStatus.Failed;

        await _prizeAwardRepository.UpdateAsync(award, cancellationToken);
    }

    private static PrizeAwardResponse MapToResponse(PrizeAward award)
    {
        var userName = award.ExternalUser is not null
            ? $"{award.ExternalUser.FirstName} {award.ExternalUser.LastName}".Trim()
            : null;

        return new PrizeAwardResponse
        {
            PrizeAwardId = award.PrizeAwardId,
            PrizeId = award.PrizeId,
            PrizeName = award.Prize?.Name ?? string.Empty,
            PrizeTypeName = award.Prize?.PrizeType?.Name ?? string.Empty,
            CompetitionId = award.CompetitionId,
            CompetitionName = award.Competition?.Name,
            ExternalUserId = award.ExternalUserId,
            CellNumber = award.CellNumber,
            UserName = string.IsNullOrWhiteSpace(userName) ? null : userName,
            AwardedAt = award.AwardedAt,
            AwardMethod = award.AwardMethod,
            Status = award.Status,
            ExpiryDate = award.ExpiryDate,
            NotificationChannel = award.NotificationChannel,
            NotificationStatus = award.NotificationStatus,
            IsRedeemable = award.IsRedeemable,
        };
    }

    private static PrizeAwardDetailResponse MapToDetailResponse(PrizeAward award)
    {
        var userName = award.ExternalUser is not null
            ? $"{award.ExternalUser.FirstName} {award.ExternalUser.LastName}".Trim()
            : null;

        return new PrizeAwardDetailResponse
        {
            PrizeAwardId = award.PrizeAwardId,
            PrizeId = award.PrizeId,
            PrizeName = award.Prize?.Name ?? string.Empty,
            PrizeTypeName = award.Prize?.PrizeType?.Name ?? string.Empty,
            CompetitionId = award.CompetitionId,
            CompetitionName = award.Competition?.Name,
            ExternalUserId = award.ExternalUserId,
            CellNumber = award.CellNumber,
            UserName = string.IsNullOrWhiteSpace(userName) ? null : userName,
            AwardedAt = award.AwardedAt,
            AwardMethod = award.AwardMethod,
            Status = award.Status,
            ExpiryDate = award.ExpiryDate,
            NotificationChannel = award.NotificationChannel,
            NotificationStatus = award.NotificationStatus,
            IsRedeemable = award.IsRedeemable,
            AwardedBySubjectId = award.AwardedBySubjectId,
            ExternalReference = award.ExternalReference,
            PrizeValue = award.Prize?.MonetaryValue,
            Redemption = award.PrizeRedemption is not null ? new PrizeRedemptionResponse
            {
                PrizeRedemptionId = award.PrizeRedemption.PrizeRedemptionId,
                PrizeAwardId = award.PrizeRedemption.PrizeAwardId,
                RedeemedAt = award.PrizeRedemption.RedeemedAt,
                RedeemedChannel = award.PrizeRedemption.RedeemedChannel,
                RedeemedFromIp = award.PrizeRedemption.RedeemedFromIp,
                RedemptionStatus = award.PrizeRedemption.RedemptionStatus,
                Notes = award.PrizeRedemption.Notes,
            } : null,
        };
    }
}
