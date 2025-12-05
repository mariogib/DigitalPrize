using System.Globalization;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Models.Dtos.Registrations;
using DigitalPrizes.Api.Repositories.Interfaces;
using DigitalPrizes.Api.Services.Interfaces;

namespace DigitalPrizes.Api.Services.Implementations;

/// <summary>
/// Service implementation for registration operations.
/// </summary>
public class RegistrationService : IRegistrationService
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IExternalUserRepository _externalUserRepository;
    private readonly IOtpService _otpService;
    private readonly IAuditService _auditService;
    private readonly ILogger<RegistrationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationService"/> class.
    /// </summary>
    /// <param name="registrationRepository">The registration repository.</param>
    /// <param name="competitionRepository">The competition repository.</param>
    /// <param name="externalUserRepository">The external user repository.</param>
    /// <param name="otpService">The OTP service.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="logger">The logger.</param>
    public RegistrationService(
        IRegistrationRepository registrationRepository,
        ICompetitionRepository competitionRepository,
        IExternalUserRepository externalUserRepository,
        IOtpService otpService,
        IAuditService auditService,
        ILogger<RegistrationService> logger)
    {
        _registrationRepository = registrationRepository;
        _competitionRepository = competitionRepository;
        _externalUserRepository = externalUserRepository;
        _otpService = otpService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PublicRegistrationResponse> RegisterAsync(
        PublicRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate competition
        var competition = await _competitionRepository.GetWithFieldsAsync(request.CompetitionId, cancellationToken);

        if (competition is null)
        {
            return new PublicRegistrationResponse
            {
                CellNumber = request.CellNumber,
                Message = "Competition not found.",
            };
        }

        if (!competition.IsActive)
        {
            return new PublicRegistrationResponse
            {
                CellNumber = request.CellNumber,
                Message = "Competition is not accepting registrations.",
            };
        }

        // Check if already registered
        var existingRegistration = await _registrationRepository.GetByCompetitionAndCellAsync(
            request.CompetitionId,
            request.CellNumber,
            cancellationToken);

        if (existingRegistration is not null)
        {
            return new PublicRegistrationResponse
            {
                CellNumber = request.CellNumber,
                Message = "You are already registered for this competition.",
            };
        }

        // Send OTP for verification
        var otpResult = await _otpService.SendOtpAsync(
            request.CellNumber,
            OtpPurpose.Registration,
            request.CompetitionId,
            cancellationToken);

        if (!otpResult.Success)
        {
            return new PublicRegistrationResponse
            {
                CellNumber = request.CellNumber,
                RequiresOtp = true,
                Message = "Failed to send verification code. Please try again.",
            };
        }

        // Create pending registration
        var pendingRegistration = await CreateRegistrationAsync(request, cancellationToken);

        return new PublicRegistrationResponse
        {
            RegistrationId = pendingRegistration.RegistrationId,
            CellNumber = request.CellNumber,
            RequiresOtp = true,
            Message = "Verification code sent. Please enter the code to complete registration.",
        };
    }

    /// <inheritdoc/>
    public async Task<PublicRegistrationResponse> VerifyAndCompleteRegistrationAsync(
        string cellNumber,
        string otpCode,
        int competitionId,
        CancellationToken cancellationToken = default)
    {
        // Verify OTP
        var verifyResult = await _otpService.VerifyOtpAsync(
            cellNumber,
            otpCode,
            OtpPurpose.Registration,
            competitionId,
            cancellationToken);

        if (!verifyResult.IsValid)
        {
            return new PublicRegistrationResponse
            {
                CellNumber = cellNumber,
                RequiresOtp = true,
                Message = verifyResult.Message,
            };
        }

        // Find pending registration
        var registration = await _registrationRepository.GetByCompetitionAndCellAsync(
            competitionId,
            cellNumber,
            cancellationToken);

        if (registration is null)
        {
            return new PublicRegistrationResponse
            {
                CellNumber = cellNumber,
                Message = "Registration not found. Please register again.",
            };
        }

        // Registration is already complete (no status field to update in this model)
        await _auditService.LogAsync(
            AuditAction.Update,
            "Registration",
            registration.RegistrationId.ToString(CultureInfo.InvariantCulture),
            details: "Registration verified via OTP",
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Registration {RegistrationId} verified for {CellNumber}",
            registration.RegistrationId,
            cellNumber);

        return new PublicRegistrationResponse
        {
            RegistrationId = registration.RegistrationId,
            CellNumber = cellNumber,
            Message = "Registration verified and completed!",
        };
    }

    /// <inheritdoc/>
    public async Task<RegistrationResponse?> GetRegistrationAsync(
        long registrationId,
        CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetWithAnswersAsync(registrationId, cancellationToken);

        if (registration is null)
        {
            return null;
        }

        return MapToResponse(registration);
    }

    /// <inheritdoc/>
    public async Task<PagedResponse<RegistrationResponse>> GetRegistrationsAsync(
        FilterParameters parameters,
        int? competitionId = null,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _registrationRepository.GetPagedAsync(parameters, competitionId, cancellationToken);
        return new PagedResponse<RegistrationResponse>
        {
            Items = pagedResult.Items.Select(MapToResponse).ToList(),
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
        };
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RegistrationResponse>> GetCompetitionRegistrationsAsync(
        int competitionId,
        CancellationToken cancellationToken = default)
    {
        var registrations = await _registrationRepository.GetByCompetitionAsync(competitionId, cancellationToken);
        return registrations.Select(MapToResponse).ToList();
    }

    /// <inheritdoc/>
    public async Task<bool> IsRegisteredAsync(
        int competitionId,
        string cellNumber,
        CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetByCompetitionAndCellAsync(
            competitionId,
            cellNumber,
            cancellationToken);

        return registration is not null;
    }

    /// <inheritdoc/>
    public async Task<BulkRegistrationResult> BulkRegisterAsync(
        int competitionId,
        IEnumerable<AdminRegistrationRequest> registrations,
        string? subjectId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registrations);

        var competition = await _competitionRepository.GetByIdAsync(competitionId, cancellationToken);

        if (competition is null)
        {
            return new BulkRegistrationResult
            {
                TotalRequested = 0,
                Results = new List<BulkRegistrationItemResult>
                {
                    new BulkRegistrationItemResult { Success = false, ErrorMessage = "Competition not found." },
                },
            };
        }

        var results = new List<BulkRegistrationItemResult>();
        var successCount = 0;
        var failCount = 0;
        var duplicateCount = 0;

        foreach (var request in registrations)
        {
            try
            {
                // Check for duplicate
                var existing = await _registrationRepository.GetByCompetitionAndCellAsync(
                    competitionId,
                    request.CellNumber,
                    cancellationToken);

                if (existing is not null)
                {
                    results.Add(new BulkRegistrationItemResult
                    {
                        CellNumber = request.CellNumber,
                        Success = false,
                        ErrorMessage = "Already registered.",
                    });
                    duplicateCount++;
                    continue;
                }

                // Get or create external user
                var externalUser = await GetOrCreateExternalUserAsync(
                    request.CellNumber,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    cancellationToken);

                // Create registration
                var registration = new Registration
                {
                    CompetitionId = competitionId,
                    ExternalUserId = externalUser.ExternalUserId,
                    CellNumber = request.CellNumber,
                    RegistrationDate = DateTime.UtcNow,
                    Source = RegistrationChannelType.Import,
                    ConsentGiven = true,
                };

                // Add answers from FieldAnswers dictionary
                foreach (var (fieldId, value) in request.FieldAnswers)
                {
                    registration.RegistrationAnswers.Add(new RegistrationAnswer
                    {
                        RegistrationFieldId = fieldId,
                        Value = value,
                    });
                }

                await _registrationRepository.AddAsync(registration, cancellationToken);

                results.Add(new BulkRegistrationItemResult
                {
                    CellNumber = request.CellNumber,
                    Success = true,
                    RegistrationId = registration.RegistrationId,
                });
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register {CellNumber}", request.CellNumber);
                results.Add(new BulkRegistrationItemResult
                {
                    CellNumber = request.CellNumber,
                    Success = false,
                    ErrorMessage = ex.Message,
                });
                failCount++;
            }
        }

        await _auditService.LogAsync(
            AuditAction.Create,
            "Registration",
            "Bulk",
            subjectId,
            $"Bulk registration for competition {competitionId}: {successCount} success, {failCount} failed, {duplicateCount} duplicates",
            cancellationToken: cancellationToken);

        return new BulkRegistrationResult
        {
            TotalRequested = results.Count,
            SuccessfulRegistrations = successCount,
            FailedRegistrations = failCount,
            DuplicateRegistrations = duplicateCount,
            Results = results,
        };
    }

    private static RegistrationResponse MapToResponse(Registration registration)
    {
        return new RegistrationResponse
        {
            RegistrationId = registration.RegistrationId,
            CompetitionId = registration.CompetitionId,
            CompetitionName = registration.Competition?.Name ?? string.Empty,
            ExternalUserId = registration.ExternalUserId,
            CellNumber = registration.CellNumber,
            RegisteredAt = registration.RegistrationDate,
            RegistrationChannel = registration.Source ?? string.Empty,
            Answers = registration.RegistrationAnswers.Select(a => new RegistrationAnswerResponse
            {
                RegistrationAnswerId = a.RegistrationAnswerId,
                RegistrationFieldId = a.RegistrationFieldId,
                FieldName = a.RegistrationField?.FieldName ?? string.Empty,
                Value = a.Value ?? string.Empty,
            }).ToList(),
        };
    }

    private async Task<Registration> CreateRegistrationAsync(
        PublicRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        // Get or create external user
        var externalUser = await GetOrCreateExternalUserAsync(
            request.CellNumber,
            request.FirstName,
            request.LastName,
            request.Email,
            cancellationToken);

        // Create registration
        var registration = new Registration
        {
            CompetitionId = request.CompetitionId,
            ExternalUserId = externalUser.ExternalUserId,
            CellNumber = request.CellNumber,
            RegistrationDate = DateTime.UtcNow,
            Source = RegistrationChannelType.Web,
            ConsentGiven = request.AcceptsTerms,
        };

        // Add answers from FieldAnswers dictionary
        foreach (var (fieldId, value) in request.FieldAnswers)
        {
            registration.RegistrationAnswers.Add(new RegistrationAnswer
            {
                RegistrationFieldId = fieldId,
                Value = value,
            });
        }

        await _registrationRepository.AddAsync(registration, cancellationToken);
        return registration;
    }

    private async Task<ExternalUser> GetOrCreateExternalUserAsync(
        string cellNumber,
        string? firstName,
        string? lastName,
        string? email,
        CancellationToken cancellationToken)
    {
        var user = await _externalUserRepository.GetByCellNumberAsync(cellNumber, cancellationToken);

        if (user is not null)
        {
            // Update user details if provided
            var updated = false;

            if (!string.IsNullOrWhiteSpace(firstName) && user.FirstName != firstName)
            {
                user.FirstName = firstName;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(lastName) && user.LastName != lastName)
            {
                user.LastName = lastName;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(email) && user.Email != email)
            {
                user.Email = email;
                updated = true;
            }

            if (updated)
            {
                user.UpdatedAt = DateTime.UtcNow;
                await _externalUserRepository.UpdateAsync(user, cancellationToken);
            }

            return user;
        }

        user = new ExternalUser
        {
            CellNumber = cellNumber,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };

        await _externalUserRepository.AddAsync(user, cancellationToken);
        return user;
    }
}
