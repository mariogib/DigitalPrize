using System.Globalization;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Prizes;
using DigitalPrizes.Api.Repositories.Interfaces;
using DigitalPrizes.Api.Services.Interfaces;

namespace DigitalPrizes.Api.Services.Implementations;

/// <summary>
/// Service implementation for prize pool management.
/// </summary>
public class PrizePoolService : IPrizePoolService
{
    private readonly IPrizePoolRepository _prizePoolRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<PrizePoolService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrizePoolService"/> class.
    /// </summary>
    /// <param name="prizePoolRepository">The prize pool repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="logger">The logger.</param>
    public PrizePoolService(
        IPrizePoolRepository prizePoolRepository,
        IAuditService auditService,
        ILogger<PrizePoolService> logger)
    {
        _prizePoolRepository = prizePoolRepository;
        _auditService = auditService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PagedResponse<PrizePoolSummaryResponse>> GetPrizePoolsAsync(
        FilterParameters parameters,
        int? competitionId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var result = await _prizePoolRepository.GetPagedAsync(parameters, isActive, cancellationToken);

        var items = result.Items.Select(MapToSummaryResponse).ToList();

        return new PagedResponse<PrizePoolSummaryResponse>
        {
            Items = items,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
        };
    }

    /// <inheritdoc/>
    public async Task<PrizePoolDetailResponse?> GetPrizePoolAsync(int prizePoolId, CancellationToken cancellationToken = default)
    {
        var prizePool = await _prizePoolRepository.GetWithPrizesAsync(prizePoolId, cancellationToken);

        if (prizePool is null)
        {
            return null;
        }

        return new PrizePoolDetailResponse
        {
            PrizePoolId = prizePool.PrizePoolId,
            Name = prizePool.Name,
            Description = prizePool.Description,
            IsActive = prizePool.IsActive,
            TotalPrizes = prizePool.Prizes.Sum(p => p.TotalQuantity),
            AvailablePrizes = prizePool.Prizes.Sum(p => p.RemainingQuantity),
            AwardedPrizes = prizePool.Prizes.Sum(p => p.TotalQuantity - p.RemainingQuantity),
            RedeemedPrizes = prizePool.Prizes.Sum(p => p.PrizeAwards.Count(a => a.Status == "Redeemed")),
            CreatedAt = prizePool.CreatedAt,
            Prizes = prizePool.Prizes.Select(p => new PrizeSummaryResponse
            {
                PrizeId = p.PrizeId,
                PrizePoolId = p.PrizePoolId,
                PrizePoolName = prizePool.Name,
                PrizeTypeId = p.PrizeTypeId,
                PrizeTypeName = p.PrizeType?.Name ?? string.Empty,
                Name = p.Name,
                MonetaryValue = p.MonetaryValue,
                TotalQuantity = p.TotalQuantity,
                RemainingQuantity = p.RemainingQuantity,
                AwardedQuantity = p.TotalQuantity - p.RemainingQuantity,
                RedeemedQuantity = p.PrizeAwards.Count(a => a.Status == "Redeemed"),
                IsActive = p.IsActive,
                ExpiryDate = p.ExpiryDate,
            }).ToList(),
        };
    }

    /// <inheritdoc/>
    public async Task<PrizePoolSummaryResponse> CreatePrizePoolAsync(
        CreatePrizePoolRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var prizePool = new PrizePool
        {
            CompetitionId = request.CompetitionId,
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        await _prizePoolRepository.AddAsync(prizePool, cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Create,
            "PrizePool",
            prizePool.PrizePoolId.ToString(CultureInfo.InvariantCulture),
            subjectId,
            $"Created prize pool: {prizePool.Name}",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Created prize pool {PrizePoolId}: {Name}", prizePool.PrizePoolId, prizePool.Name);

        return MapToSummaryResponse(prizePool);
    }

    /// <inheritdoc/>
    public async Task<PrizePoolSummaryResponse?> UpdatePrizePoolAsync(
        int prizePoolId,
        UpdatePrizePoolRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var prizePool = await _prizePoolRepository.GetByIdAsync(prizePoolId, cancellationToken);

        if (prizePool is null)
        {
            return null;
        }

        prizePool.Name = request.Name;
        prizePool.Description = request.Description;
        prizePool.IsActive = request.IsActive;

        await _prizePoolRepository.UpdateAsync(prizePool, cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Update,
            "PrizePool",
            prizePoolId.ToString(CultureInfo.InvariantCulture),
            subjectId,
            $"Updated prize pool: {prizePool.Name}",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Updated prize pool {PrizePoolId}", prizePoolId);

        return MapToSummaryResponse(prizePool);
    }

    /// <inheritdoc/>
    public async Task<bool> DeletePrizePoolAsync(int prizePoolId, string? subjectId = null, CancellationToken cancellationToken = default)
    {
        var prizePool = await _prizePoolRepository.GetByIdAsync(prizePoolId, cancellationToken);

        if (prizePool is null)
        {
            return false;
        }

        await _prizePoolRepository.DeleteAsync(prizePool, cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Delete,
            "PrizePool",
            prizePoolId.ToString(CultureInfo.InvariantCulture),
            subjectId,
            $"Deleted prize pool: {prizePool.Name}",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Deleted prize pool {PrizePoolId}", prizePoolId);

        return true;
    }

    private static PrizePoolSummaryResponse MapToSummaryResponse(PrizePool prizePool)
    {
        return new PrizePoolSummaryResponse
        {
            PrizePoolId = prizePool.PrizePoolId,
            CompetitionId = prizePool.CompetitionId,
            CompetitionName = prizePool.Competition?.Name,
            Name = prizePool.Name,
            IsActive = prizePool.IsActive,
            TotalPrizes = prizePool.Prizes?.Sum(p => p.TotalQuantity) ?? 0,
            AvailablePrizes = prizePool.Prizes?.Sum(p => p.RemainingQuantity) ?? 0,
            AwardedPrizes = prizePool.Prizes?.Sum(p => p.TotalQuantity - p.RemainingQuantity) ?? 0,
            RedeemedPrizes = prizePool.Prizes?.Sum(p => p.PrizeAwards.Count(a => a.Status == "Redeemed")) ?? 0,
        };
    }
}
