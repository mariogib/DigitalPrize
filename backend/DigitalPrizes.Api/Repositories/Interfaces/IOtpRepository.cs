using DigitalPrizes.Api.Models.Domain;

namespace DigitalPrizes.Api.Repositories.Interfaces;

/// <summary>
/// Repository interface for Otp entities.
/// </summary>
public interface IOtpRepository : IRepository<Otp, long>
{
    /// <summary>
    /// Gets the latest valid OTP for a cell number and purpose.
    /// </summary>
    Task<Otp?> GetLatestValidAsync(
        string cellNumber,
        string purpose,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an OTP by code, cell number, and purpose.
    /// </summary>
    Task<Otp?> GetByCodeAsync(
        string cellNumber,
        string code,
        string purpose,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all existing OTPs for a cell number and purpose.
    /// </summary>
    Task InvalidateExistingAsync(
        string cellNumber,
        string purpose,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments the attempt count for an OTP.
    /// </summary>
    Task<int> IncrementAttemptCountAsync(
        long otpId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an OTP as used.
    /// </summary>
    Task MarkAsUsedAsync(
        long otpId,
        CancellationToken cancellationToken = default);
}
