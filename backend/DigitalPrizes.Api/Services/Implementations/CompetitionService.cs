using System.Globalization;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Competitions;
using DigitalPrizes.Api.Models.Dtos.Prizes;
using DigitalPrizes.Api.Repositories.Interfaces;
using DigitalPrizes.Api.Services.Interfaces;

namespace DigitalPrizes.Api.Services.Implementations;

/// <summary>
/// Service implementation for competition management.
/// </summary>
public class CompetitionService : ICompetitionService
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<CompetitionService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompetitionService"/> class.
    /// </summary>
    /// <param name="competitionRepository">The competition repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="logger">The logger.</param>
    public CompetitionService(
        ICompetitionRepository competitionRepository,
        IAuditService auditService,
        ILogger<CompetitionService> logger)
    {
        _competitionRepository = competitionRepository;
        _auditService = auditService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PagedResponse<CompetitionResponse>> GetCompetitionsAsync(
        FilterParameters parameters,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var result = await _competitionRepository.GetPagedAsync(parameters, status, cancellationToken);

        var items = result.Items.Select(MapToResponse).ToList();

        return new PagedResponse<CompetitionResponse>
        {
            Items = items,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
        };
    }

    /// <inheritdoc/>
    public async Task<CompetitionDetailResponse?> GetCompetitionAsync(int competitionId, CancellationToken cancellationToken = default)
    {
        var competition = await _competitionRepository.GetWithFieldsAsync(competitionId, cancellationToken);

        if (competition is null)
        {
            return null;
        }

        return new CompetitionDetailResponse
        {
            CompetitionId = competition.CompetitionId,
            Name = competition.Name,
            Description = competition.Description,
            StartDate = competition.StartDate,
            EndDate = competition.EndDate,
            Status = competition.IsActive ? "Active" : "Inactive",
            CreatedAt = competition.CreatedAt,
            RegistrationCount = competition.Registrations?.Count ?? 0,
            PrizePoolCount = competition.PrizePool != null ? 1 : 0,
            AwardedPrizesCount = 0,
            RegistrationFields = competition.RegistrationFields?
                .OrderBy(f => f.DisplayOrder)
                .Select(f => new RegistrationFieldResponse
                {
                    RegistrationFieldId = f.RegistrationFieldId,
                    CompetitionId = f.CompetitionId,
                    FieldName = f.FieldName,
                    FieldType = f.FieldType,
                    IsRequired = f.IsRequired,
                    DisplayOrder = f.DisplayOrder,
                    Options = f.OptionsJson,
                    ValidationRules = f.ValidationRules,
                }).ToList() ?? new List<RegistrationFieldResponse>(),
            PrizePools = competition.PrizePool != null
                ? new List<PrizePoolSummaryResponse>
                {
                    new PrizePoolSummaryResponse
                    {
                        PrizePoolId = competition.PrizePool.PrizePoolId,
                        CompetitionId = competition.PrizePool.CompetitionId,
                        CompetitionName = competition.Name,
                        Name = competition.PrizePool.Name,
                        IsActive = competition.PrizePool.IsActive,
                        TotalPrizes = competition.PrizePool.Prizes?.Sum(p => p.TotalQuantity) ?? 0,
                        AvailablePrizes = competition.PrizePool.Prizes?.Sum(p => p.RemainingQuantity) ?? 0,
                        AwardedPrizes = competition.PrizePool.Prizes?.Sum(p => p.TotalQuantity - p.RemainingQuantity) ?? 0,
                        RedeemedPrizes = 0,
                    }
                }
                : new List<PrizePoolSummaryResponse>(),
        };
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CompetitionResponse>> GetActiveCompetitionsAsync(CancellationToken cancellationToken = default)
    {
        var competitions = await _competitionRepository.GetActiveCompetitionsAsync(cancellationToken);
        return competitions.Select(MapToResponse).ToList();
    }

    /// <inheritdoc/>
    public async Task<CompetitionResponse> CreateCompetitionAsync(
        CreateCompetitionRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var competition = new Competition
        {
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = false, // New competitions start as inactive
            CreatedAt = DateTime.UtcNow,
            RegistrationFields = request.RegistrationFields?.Select((f, index) => new RegistrationField
            {
                FieldName = f.FieldName,
                Label = f.FieldName,
                FieldType = f.FieldType,
                IsRequired = f.IsRequired,
                DisplayOrder = f.DisplayOrder > 0 ? f.DisplayOrder : index + 1,
                ValidationRules = f.ValidationRules,
                OptionsJson = f.Options,
                IsActive = true,
            }).ToList() ?? new List<RegistrationField>(),
        };

        await _competitionRepository.AddAsync(competition, cancellationToken);

        await _auditService.LogCompetitionActionAsync(
            AuditAction.Create,
            competition.CompetitionId,
            subjectId,
            $"Created competition: {competition.Name}",
            cancellationToken);

        _logger.LogInformation("Created competition {CompetitionId}: {CompetitionName}", competition.CompetitionId, competition.Name);

        return MapToResponse(competition);
    }

    /// <inheritdoc/>
    public async Task<CompetitionResponse?> UpdateCompetitionAsync(
        int competitionId,
        UpdateCompetitionRequest request,
        string? subjectId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var competition = await _competitionRepository.GetByIdAsync(competitionId, cancellationToken);

        if (competition is null)
        {
            return null;
        }

        competition.Name = request.Name;
        competition.Description = request.Description;
        competition.StartDate = request.StartDate;
        competition.EndDate = request.EndDate;
        competition.IsActive = string.Equals(request.Status, "Active", StringComparison.OrdinalIgnoreCase);

        await _competitionRepository.UpdateAsync(competition, cancellationToken);

        await _auditService.LogCompetitionActionAsync(
            AuditAction.Update,
            competitionId,
            subjectId,
            $"Updated competition: {competition.Name}",
            cancellationToken);

        _logger.LogInformation("Updated competition {CompetitionId}", competitionId);

        return MapToResponse(competition);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateStatusAsync(
        int competitionId,
        string newStatus,
        string? subjectId = null,
        CancellationToken cancellationToken = default)
    {
        var competition = await _competitionRepository.GetByIdAsync(competitionId, cancellationToken);

        if (competition is null)
        {
            return false;
        }

        var oldStatus = competition.IsActive ? "Active" : "Inactive";
        competition.IsActive = string.Equals(newStatus, "Active", StringComparison.OrdinalIgnoreCase);

        await _competitionRepository.UpdateAsync(competition, cancellationToken);

        await _auditService.LogCompetitionActionAsync(
            AuditAction.Update,
            competitionId,
            subjectId,
            $"Status changed from {oldStatus} to {newStatus}",
            cancellationToken);

        _logger.LogInformation("Competition {CompetitionId} status changed to {Status}", competitionId, newStatus);

        return true;
    }

    /// <inheritdoc/>
    public async Task<RegistrationFieldResponse?> AddRegistrationFieldAsync(
        int competitionId,
        CreateRegistrationFieldRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var competition = await _competitionRepository.GetWithFieldsAsync(competitionId, cancellationToken);

        if (competition is null)
        {
            return null;
        }

        var maxOrder = competition.RegistrationFields?.Max(f => f.DisplayOrder) ?? 0;

        var field = new RegistrationField
        {
            CompetitionId = competitionId,
            FieldName = request.FieldName,
            Label = request.FieldName,
            FieldType = request.FieldType,
            IsRequired = request.IsRequired,
            DisplayOrder = request.DisplayOrder > 0 ? request.DisplayOrder : maxOrder + 1,
            ValidationRules = request.ValidationRules,
            OptionsJson = request.Options,
            IsActive = true,
        };

        competition.RegistrationFields ??= new List<RegistrationField>();
        competition.RegistrationFields.Add(field);

        await _competitionRepository.UpdateAsync(competition, cancellationToken);

        return new RegistrationFieldResponse
        {
            RegistrationFieldId = field.RegistrationFieldId,
            CompetitionId = field.CompetitionId,
            FieldName = field.FieldName,
            FieldType = field.FieldType,
            IsRequired = field.IsRequired,
            DisplayOrder = field.DisplayOrder,
            Options = field.OptionsJson,
            ValidationRules = field.ValidationRules,
        };
    }

    /// <inheritdoc/>
    public async Task<RegistrationFieldResponse?> UpdateRegistrationFieldAsync(
        int registrationFieldId,
        UpdateRegistrationFieldRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var field = await _competitionRepository.GetFieldByIdAsync(registrationFieldId, cancellationToken);

        if (field is null)
        {
            return null;
        }

        field.FieldName = request.FieldName;
        field.Label = request.FieldName;
        field.FieldType = request.FieldType;
        field.IsRequired = request.IsRequired;
        field.DisplayOrder = request.DisplayOrder;
        field.ValidationRules = request.ValidationRules;
        field.OptionsJson = request.Options;

        await _competitionRepository.UpdateFieldAsync(field, cancellationToken);

        return new RegistrationFieldResponse
        {
            RegistrationFieldId = field.RegistrationFieldId,
            CompetitionId = field.CompetitionId,
            FieldName = field.FieldName,
            FieldType = field.FieldType,
            IsRequired = field.IsRequired,
            DisplayOrder = field.DisplayOrder,
            Options = field.OptionsJson,
            ValidationRules = field.ValidationRules,
        };
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteRegistrationFieldAsync(int registrationFieldId, CancellationToken cancellationToken = default)
    {
        var field = await _competitionRepository.GetFieldByIdAsync(registrationFieldId, cancellationToken);

        if (field is null)
        {
            return false;
        }

        await _competitionRepository.DeleteFieldAsync(field, cancellationToken);
        return true;
    }

    private static CompetitionResponse MapToResponse(Competition competition)
    {
        return new CompetitionResponse
        {
            CompetitionId = competition.CompetitionId,
            Name = competition.Name,
            Description = competition.Description,
            StartDate = competition.StartDate,
            EndDate = competition.EndDate,
            Status = competition.IsActive ? "Active" : "Inactive",
            CreatedAt = competition.CreatedAt,
            RegistrationCount = competition.Registrations?.Count ?? 0,
            PrizePoolCount = 0,
            AwardedPrizesCount = 0,
        };
    }
}
