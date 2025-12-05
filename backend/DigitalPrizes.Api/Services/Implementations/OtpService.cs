using System.Globalization;
using System.Security.Cryptography;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Otp;
using DigitalPrizes.Api.Repositories.Interfaces;
using DigitalPrizes.Api.Services.Interfaces;

namespace DigitalPrizes.Api.Services.Implementations;

/// <summary>
/// Service implementation for OTP operations.
/// </summary>
public class OtpService : IOtpService
{
    private const int OtpDigits = 6;
    private const int OtpExpiryMinutes = 5;
    private const int OtpMaxAttempts = 3;

    private readonly IOtpRepository _otpRepository;
    private readonly ISmsService _smsService;
    private readonly ILogger<OtpService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OtpService"/> class.
    /// </summary>
    /// <param name="otpRepository">The OTP repository.</param>
    /// <param name="smsService">The SMS service.</param>
    /// <param name="logger">The logger.</param>
    public OtpService(
        IOtpRepository otpRepository,
        ISmsService smsService,
        ILogger<OtpService> logger)
    {
        _otpRepository = otpRepository;
        _smsService = smsService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SendOtpResponse> SendOtpAsync(
        string cellNumber,
        string purpose,
        long? relatedEntityId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cellNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);

        // Invalidate any existing OTPs for this cell number and purpose
        await _otpRepository.InvalidateExistingAsync(cellNumber, purpose, cancellationToken);

        // Generate new OTP
        var otpCode = GenerateSecureOtpCode();
        var expiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);

        var otp = new Otp
        {
            CellNumber = cellNumber,
            Code = otpCode,
            Purpose = purpose,
            RelatedEntityId = relatedEntityId,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            IsUsed = false,
            AttemptCount = 0,
        };

        await _otpRepository.AddAsync(otp, cancellationToken);

        // Send SMS
        var smsResult = await _smsService.SendOtpSmsAsync(cellNumber, otpCode, cancellationToken);

        if (!smsResult.Success)
        {
            _logger.LogWarning("Failed to send OTP SMS to {CellNumber}: {Error}", cellNumber, smsResult.ErrorMessage);
            return new SendOtpResponse
            {
                Success = false,
                Message = "Failed to send OTP. Please try again.",
            };
        }

        _logger.LogInformation("OTP sent to {CellNumber} for {Purpose}", cellNumber, purpose);

        return new SendOtpResponse
        {
            Success = true,
            Message = "OTP sent successfully.",
            ExpiresInSeconds = OtpExpiryMinutes * 60,
        };
    }

    /// <inheritdoc/>
    public async Task<VerifyOtpResponse> VerifyOtpAsync(
        string cellNumber,
        string code,
        string purpose,
        long? relatedEntityId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cellNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);

        var otp = await _otpRepository.GetLatestValidAsync(cellNumber, purpose, cancellationToken);

        if (otp is null)
        {
            _logger.LogWarning("No valid OTP found for {CellNumber} and {Purpose}", cellNumber, purpose);
            return new VerifyOtpResponse
            {
                IsValid = false,
                Message = "No valid OTP found. Please request a new OTP.",
            };
        }

        // Check if max attempts exceeded
        if (otp.AttemptCount >= OtpMaxAttempts)
        {
            _logger.LogWarning("Max OTP attempts exceeded for {CellNumber}", cellNumber);
            await _otpRepository.MarkAsUsedAsync(otp.OtpId, cancellationToken);

            return new VerifyOtpResponse
            {
                IsValid = false,
                Message = "Maximum attempts exceeded. Please request a new OTP.",
                RemainingAttempts = 0,
            };
        }

        // Check if OTP expired
        if (otp.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("OTP expired for {CellNumber}", cellNumber);
            return new VerifyOtpResponse
            {
                IsValid = false,
                Message = "OTP has expired. Please request a new OTP.",
            };
        }

        // Verify OTP code
        if (!string.Equals(otp.Code, code, StringComparison.OrdinalIgnoreCase))
        {
            var attemptCount = await _otpRepository.IncrementAttemptCountAsync(otp.OtpId, cancellationToken);

            _logger.LogWarning("Invalid OTP attempt for {CellNumber}, attempt {Attempt}", cellNumber, attemptCount);

            return new VerifyOtpResponse
            {
                IsValid = false,
                Message = "Invalid OTP code.",
                RemainingAttempts = OtpMaxAttempts - attemptCount,
            };
        }

        // Mark OTP as used
        await _otpRepository.MarkAsUsedAsync(otp.OtpId, cancellationToken);

        _logger.LogInformation("OTP verified successfully for {CellNumber}", cellNumber);

        return new VerifyOtpResponse
        {
            IsValid = true,
            Message = "OTP verified successfully.",
            OtpId = otp.OtpId,
        };
    }

    /// <inheritdoc/>
    public async Task<long?> GetVerifiedOtpIdAsync(
        string cellNumber,
        string code,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        var result = await VerifyOtpAsync(cellNumber, code, purpose, cancellationToken: cancellationToken);
        return result.IsValid ? result.OtpId : null;
    }

    private static string GenerateSecureOtpCode()
    {
        // Use cryptographically secure random number generator
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var number = BitConverter.ToUInt32(bytes, 0) % (uint)Math.Pow(10, OtpDigits);
        return number.ToString(CultureInfo.InvariantCulture).PadLeft(OtpDigits, '0');
    }
}
