using System.Globalization;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Awards;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Repositories.Interfaces;
using DigitalPrizes.Api.Services.Interfaces;

namespace DigitalPrizes.Api.Services.Implementations;

/// <summary>
/// Service implementation for prize redemption operations.
/// </summary>
public class RedemptionService : IRedemptionService
{
    private readonly IPrizeAwardRepository _prizeAwardRepository;
    private readonly IPrizeRedemptionRepository _prizeRedemptionRepository;
    private readonly IOtpService _otpService;
    private readonly ISmsService _smsService;
    private readonly IAuditService _auditService;
    private readonly ILogger<RedemptionService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedemptionService"/> class.
    /// </summary>
    /// <param name="prizeAwardRepository">The prize award repository.</param>
    /// <param name="prizeRedemptionRepository">The prize redemption repository.</param>
    /// <param name="otpService">The OTP service.</param>
    /// <param name="smsService">The SMS service.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="logger">The logger.</param>
    public RedemptionService(
        IPrizeAwardRepository prizeAwardRepository,
        IPrizeRedemptionRepository prizeRedemptionRepository,
        IOtpService otpService,
        ISmsService smsService,
        IAuditService auditService,
        ILogger<RedemptionService> logger)
    {
        _prizeAwardRepository = prizeAwardRepository;
        _prizeRedemptionRepository = prizeRedemptionRepository;
        _otpService = otpService;
        _smsService = smsService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PagedResponse<PrizeRedemptionResponse>> GetRedemptionsAsync(
        FilterParameters parameters,
        int? competitionId = null,
        int? prizePoolId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _prizeRedemptionRepository.GetPagedAsync(
            parameters,
            competitionId,
            prizePoolId,
            cancellationToken);

        return new PagedResponse<PrizeRedemptionResponse>
        {
            Items = result.Items.Select(MapToResponse).ToList(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
        };
    }

    /// <inheritdoc/>
    public async Task<InitiateRedemptionResponse> InitiateRedemptionAsync(
        InitiateRedemptionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CellNumber);

        // Get redeemable prizes for the cell number
        var awards = await _prizeAwardRepository.GetRedeemableAsync(request.CellNumber, cancellationToken);

        if (awards.Count == 0)
        {
            return new InitiateRedemptionResponse
            {
                RequiresOtp = false,
                Message = "No prizes available for redemption.",
                RedeemablePrizes = new List<RedeemablePrizeResponse>(),
            };
        }

        // Filter to specific award if provided
        if (request.PrizeAwardId.HasValue)
        {
            awards = awards.Where(a => a.PrizeAwardId == request.PrizeAwardId.Value).ToList();
            if (awards.Count == 0)
            {
                return new InitiateRedemptionResponse
                {
                    RequiresOtp = false,
                    Message = "The specified prize is not available for redemption.",
                    RedeemablePrizes = new List<RedeemablePrizeResponse>(),
                };
            }
        }

        // Send OTP
        var otpResult = await _otpService.SendOtpAsync(
            request.CellNumber,
            OtpPurpose.Redemption,
            cancellationToken: cancellationToken);

        if (!otpResult.Success)
        {
            return new InitiateRedemptionResponse
            {
                RequiresOtp = true,
                Message = "Failed to send verification code. Please try again.",
                RedeemablePrizes = new List<RedeemablePrizeResponse>(),
            };
        }

        _logger.LogInformation(
            "Redemption initiated for {CellNumber}, {Count} prizes available",
            request.CellNumber,
            awards.Count);

        return new InitiateRedemptionResponse
        {
            RequiresOtp = true,
            Message = $"Verification code sent to {MaskCellNumber(request.CellNumber)}",
            RedeemablePrizes = awards.Select(a => new RedeemablePrizeResponse
            {
                PrizeAwardId = a.PrizeAwardId,
                PrizeName = a.Prize?.Name ?? string.Empty,
                PrizeTypeName = a.Prize?.PrizeType?.Name ?? string.Empty,
                MonetaryValue = a.Prize?.MonetaryValue,
                AwardedAt = a.AwardedAt,
                ExpiryDate = a.ExpiryDate,
            }).ToList(),
        };
    }

    /// <inheritdoc/>
    public async Task<CompleteRedemptionResponse> CompleteRedemptionAsync(
        CompleteRedemptionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CellNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OtpCode);

        // Verify OTP
        var otpResult = await _otpService.VerifyOtpAsync(
            request.CellNumber,
            request.OtpCode,
            OtpPurpose.Redemption,
            cancellationToken: cancellationToken);

        if (!otpResult.IsValid)
        {
            _logger.LogWarning(
                "OTP verification failed for redemption: {CellNumber}",
                request.CellNumber);

            return new CompleteRedemptionResponse
            {
                Success = false,
                Message = otpResult.Message,
            };
        }

        // Get the award
        var award = await _prizeAwardRepository.GetWithRelationsAsync(request.PrizeAwardId, cancellationToken);

        if (award is null)
        {
            return new CompleteRedemptionResponse
            {
                Success = false,
                Message = "Prize not found.",
            };
        }

        // Validate the award belongs to this cell number
        if (!string.Equals(award.CellNumber, request.CellNumber, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Cell number mismatch for redemption: expected {Expected}, got {Actual}",
                award.CellNumber,
                request.CellNumber);

            return new CompleteRedemptionResponse
            {
                Success = false,
                Message = "This prize does not belong to you.",
            };
        }

        // Validate status
        if (award.Status != AwardStatus.Awarded)
        {
            return new CompleteRedemptionResponse
            {
                Success = false,
                Message = $"Prize cannot be redeemed. Current status: {award.Status}",
            };
        }

        // Check expiry
        if (award.ExpiryDate.HasValue && award.ExpiryDate.Value < DateTime.UtcNow)
        {
            return new CompleteRedemptionResponse
            {
                Success = false,
                Message = "This prize has expired.",
            };
        }

        // Create redemption record
        var redemptionCode = GenerateRedemptionCode();

        var redemption = new PrizeRedemption
        {
            PrizeAwardId = award.PrizeAwardId,
            RedeemedAt = DateTime.UtcNow,
            RedeemedChannel = request.RedemptionChannel,
            RedemptionStatus = "Completed",
            Notes = request.Notes,
        };

        await _prizeRedemptionRepository.AddAsync(redemption, cancellationToken);

        // Update award status
        award.Status = AwardStatus.Redeemed;
        await _prizeAwardRepository.UpdateAsync(award, cancellationToken);

        // Send confirmation SMS
        if (award.Prize is not null)
        {
            await _smsService.SendRedemptionConfirmationAsync(
                request.CellNumber,
                award.Prize.Name,
                redemptionCode,
                cancellationToken);
        }

        await _auditService.LogPrizeAwardActionAsync(
            AuditAction.Redeem,
            award.PrizeAwardId,
            details: $"Prize redeemed via {request.RedemptionChannel}",
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Prize {PrizeAwardId} redeemed by {CellNumber}",
            award.PrizeAwardId,
            request.CellNumber);

        return new CompleteRedemptionResponse
        {
            Success = true,
            PrizeRedemptionId = redemption.PrizeRedemptionId,
            RedemptionCode = redemptionCode,
            Message = "Prize redeemed successfully!",
            Confirmation = new RedemptionConfirmation
            {
                PrizeName = award.Prize?.Name ?? string.Empty,
                MonetaryValue = award.Prize?.MonetaryValue,
                RedeemedAt = redemption.RedeemedAt,
                ExternalReference = redemptionCode,
            },
        };
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RedeemablePrizeResponse>> GetRedeemablePrizesAsync(
        string cellNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cellNumber);

        var awards = await _prizeAwardRepository.GetRedeemableAsync(cellNumber, cancellationToken);

        return awards.Select(a => new RedeemablePrizeResponse
        {
            PrizeAwardId = a.PrizeAwardId,
            PrizeName = a.Prize?.Name ?? string.Empty,
            PrizeTypeName = a.Prize?.PrizeType?.Name ?? string.Empty,
            MonetaryValue = a.Prize?.MonetaryValue,
            AwardedAt = a.AwardedAt,
            ExpiryDate = a.ExpiryDate,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<PrizeRedemptionResponse?> GetRedemptionAsync(
        long prizeAwardId,
        CancellationToken cancellationToken = default)
    {
        var redemption = await _prizeRedemptionRepository.GetByPrizeAwardIdAsync(prizeAwardId, cancellationToken);

        if (redemption is null)
        {
            return null;
        }

        return new PrizeRedemptionResponse
        {
            PrizeRedemptionId = redemption.PrizeRedemptionId,
            PrizeAwardId = redemption.PrizeAwardId,
            RedeemedAt = redemption.RedeemedAt,
            RedeemedChannel = redemption.RedeemedChannel,
            RedeemedFromIp = redemption.RedeemedFromIp,
            RedemptionStatus = redemption.RedemptionStatus,
            Notes = redemption.Notes,
        };
    }

    private static PrizeRedemptionResponse MapToResponse(PrizeRedemption redemption)
    {
        return new PrizeRedemptionResponse
        {
            PrizeRedemptionId = redemption.PrizeRedemptionId,
            PrizeAwardId = redemption.PrizeAwardId,
            RedeemedAt = redemption.RedeemedAt,
            RedeemedChannel = redemption.RedeemedChannel,
            RedeemedFromIp = redemption.RedeemedFromIp,
            RedemptionStatus = redemption.RedemptionStatus,
            Notes = redemption.Notes,
        };
    }

    private static string GenerateRedemptionCode()
    {
        // Generate a unique redemption code (e.g., RDM-XXXXXXXX)
        return $"RDM-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
    }

    private static string MaskCellNumber(string cellNumber)
    {
        if (cellNumber.Length <= 4)
        {
            return cellNumber;
        }

        return $"{cellNumber[..3]}***{cellNumber[^4..]}";
    }
}
