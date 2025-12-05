using System.Globalization;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Prizes;
using DigitalPrizes.Api.Repositories.Interfaces;
using DigitalPrizes.Api.Services.Interfaces;

namespace DigitalPrizes.Api.Services.Implementations;

/// <summary>
/// Service implementation for prize operations.
/// </summary>
public class PrizeService : IPrizeService
{
    private readonly IPrizeRepository _prizeRepository;
    private readonly IPrizeTypeRepository _prizeTypeRepository;
    private readonly IPrizePoolRepository _prizePoolRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<PrizeService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrizeService"/> class.
    /// </summary>
    /// <param name="prizeRepository">The prize repository.</param>
    /// <param name="prizeTypeRepository">The prize type repository.</param>
    /// <param name="prizePoolRepository">The prize pool repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="logger">The logger.</param>
    public PrizeService(
        IPrizeRepository prizeRepository,
        IPrizeTypeRepository prizeTypeRepository,
        IPrizePoolRepository prizePoolRepository,
        IAuditService auditService,
        ILogger<PrizeService> logger)
    {
        _prizeRepository = prizeRepository;
        _prizeTypeRepository = prizeTypeRepository;
        _prizePoolRepository = prizePoolRepository;
        _auditService = auditService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PagedResponse<PrizeSummaryResponse>> GetPrizesAsync(
        FilterParameters parameters,
        int? prizePoolId = null,
        int? prizeTypeId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var (prizes, totalCount) = await _prizeRepository.GetPagedTupleAsync(
            parameters.PageNumber,
            parameters.PageSize,
            prizePoolId,
            prizeTypeId,
            isActive,
            cancellationToken);

        var items = prizes.Select(MapToSummaryResponse).ToList();

        return new PagedResponse<PrizeSummaryResponse>
        {
            Items = items,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
        };
    }

    /// <inheritdoc/>
    public async Task<PrizeDetailResponse?> GetPrizeAsync(long prizeId, CancellationToken cancellationToken = default)
    {
        var prize = await _prizeRepository.GetByIdWithDetailsAsync(prizeId, cancellationToken);
        return prize is null ? null : MapToDetailResponse(prize);
    }

    /// <inheritdoc/>
    public async Task<PrizeSummaryResponse> CreatePrizeAsync(
        CreatePrizeRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default)
    {
        var prize = new Prize
        {
            PrizePoolId = request.PrizePoolId,
            PrizeTypeId = request.PrizeTypeId,
            Name = request.Name,
            Description = request.Description,
            MonetaryValue = request.MonetaryValue,
            TotalQuantity = request.TotalQuantity,
            RemainingQuantity = request.TotalQuantity,
            ImageUrl = request.ImageUrl,
            ExpiryDate = request.ExpiryDate,
            MetadataJson = request.MetadataJson,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        await _prizeRepository.AddAsync(prize, cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Create,
            "Prize",
            prize.PrizeId.ToString(CultureInfo.InvariantCulture),
            subjectId,
            $"Created prize: {prize.Name}",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Created prize {PrizeId} with name {PrizeName}", prize.PrizeId, prize.Name);

        return MapToSummaryResponse(prize);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PrizeSummaryResponse>> BulkCreatePrizesAsync(
        BulkCreatePrizesRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default)
    {
        var prizes = new List<Prize>();

        for (int i = 0; i < request.Quantity; i++)
        {
            var prize = new Prize
            {
                PrizePoolId = request.PrizePoolId,
                PrizeTypeId = request.PrizeTypeId,
                Name = $"{request.NamePrefix} #{i + 1}",
                Description = request.Description,
                MonetaryValue = request.MonetaryValue,
                TotalQuantity = 1,
                RemainingQuantity = 1,
                ExpiryDate = request.ExpiryDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };
            prizes.Add(prize);
        }

        await _prizeRepository.AddRangeAsync(prizes, cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Create,
            "Prize",
            "Bulk",
            subjectId,
            $"Bulk created {request.Quantity} prizes for pool {request.PrizePoolId}",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Bulk created {Count} prizes for pool {PrizePoolId}", request.Quantity, request.PrizePoolId);

        return prizes.Select(MapToSummaryResponse).ToList();
    }

    /// <inheritdoc/>
    public async Task<PrizeSummaryResponse?> UpdatePrizeAsync(
        long prizeId,
        UpdatePrizeRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default)
    {
        var prize = await _prizeRepository.GetByIdAsync(prizeId, cancellationToken);

        if (prize is null)
        {
            return null;
        }

        prize.PrizeTypeId = request.PrizeTypeId;
        prize.Name = request.Name;
        prize.Description = request.Description;
        prize.MonetaryValue = request.MonetaryValue;
        prize.TotalQuantity = request.TotalQuantity;
        prize.ImageUrl = request.ImageUrl;
        prize.ExpiryDate = request.ExpiryDate;
        prize.MetadataJson = request.MetadataJson;
        prize.IsActive = request.IsActive;

        await _prizeRepository.UpdateAsync(prize, cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Update,
            "Prize",
            prizeId.ToString(CultureInfo.InvariantCulture),
            subjectId,
            $"Updated prize: {prize.Name}",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Updated prize {PrizeId}", prizeId);

        return MapToSummaryResponse(prize);
    }

    /// <inheritdoc/>
    public async Task<bool> DeletePrizeAsync(long prizeId, string? subjectId = null, CancellationToken cancellationToken = default)
    {
        var prize = await _prizeRepository.GetByIdAsync(prizeId, cancellationToken);

        if (prize is null)
        {
            return false;
        }

        await _prizeRepository.DeleteAsync(prize, cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Delete,
            "Prize",
            prizeId.ToString(CultureInfo.InvariantCulture),
            subjectId,
            $"Deleted prize: {prize.Name}",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Deleted prize {PrizeId}", prizeId);

        return true;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PrizeTypeResponse>> GetPrizeTypesAsync(CancellationToken cancellationToken = default)
    {
        var prizeTypes = await _prizeTypeRepository.GetActiveAsync(cancellationToken);

        return prizeTypes.Select(pt => new PrizeTypeResponse
        {
            PrizeTypeId = pt.PrizeTypeId,
            Name = pt.Name,
            Description = pt.Description,
        }).ToList();
    }

    private static PrizeSummaryResponse MapToSummaryResponse(Prize prize)
    {
        return new PrizeSummaryResponse
        {
            PrizeId = prize.PrizeId,
            PrizePoolId = prize.PrizePoolId,
            PrizePoolName = prize.PrizePool?.Name ?? string.Empty,
            PrizeTypeId = prize.PrizeTypeId,
            PrizeTypeName = prize.PrizeType?.Name ?? string.Empty,
            Name = prize.Name,
            MonetaryValue = prize.MonetaryValue,
            TotalQuantity = prize.TotalQuantity,
            RemainingQuantity = prize.RemainingQuantity,
            AwardedQuantity = prize.TotalQuantity - prize.RemainingQuantity,
            RedeemedQuantity = prize.PrizeAwards?.Count(a => a.Status == "Redeemed") ?? 0,
            IsActive = prize.IsActive,
            ExpiryDate = prize.ExpiryDate,
        };
    }

    private static PrizeDetailResponse MapToDetailResponse(Prize prize)
    {
        return new PrizeDetailResponse
        {
            PrizeId = prize.PrizeId,
            PrizePoolId = prize.PrizePoolId,
            PrizePoolName = prize.PrizePool?.Name ?? string.Empty,
            PrizeTypeId = prize.PrizeTypeId,
            PrizeTypeName = prize.PrizeType?.Name ?? string.Empty,
            Name = prize.Name,
            Description = prize.Description,
            MonetaryValue = prize.MonetaryValue,
            TotalQuantity = prize.TotalQuantity,
            RemainingQuantity = prize.RemainingQuantity,
            AwardedQuantity = prize.TotalQuantity - prize.RemainingQuantity,
            RedeemedQuantity = prize.PrizeAwards?.Count(a => a.Status == "Redeemed") ?? 0,
            IsActive = prize.IsActive,
            ExpiryDate = prize.ExpiryDate,
            ImageUrl = prize.ImageUrl,
            MetadataJson = prize.MetadataJson,
            CreatedAt = prize.CreatedAt,
        };
    }
}
