using DigitalPrizes.Api.Models.Dtos.Otp;

namespace DigitalPrizes.Api.Services.Interfaces;

/// <summary>
/// Service interface for OTP management.
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Sends an OTP to the specified cell number.
    /// </summary>
    /// <param name="cellNumber">Cell number to send OTP to.</param>
    /// <param name="purpose">Purpose of the OTP.</param>
    /// <param name="relatedEntityId">Related entity identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Send OTP response.</returns>
    Task<SendOtpResponse> SendOtpAsync(
        string cellNumber,
        string purpose,
        long? relatedEntityId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies an OTP.
    /// </summary>
    /// <param name="cellNumber">Cell number to verify.</param>
    /// <param name="code">OTP code.</param>
    /// <param name="purpose">Purpose of the OTP.</param>
    /// <param name="relatedEntityId">Related entity identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Verification response.</returns>
    Task<VerifyOtpResponse> VerifyOtpAsync(
        string cellNumber,
        string code,
        string purpose,
        long? relatedEntityId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the OTP ID if verification was successful.
    /// </summary>
    /// <param name="cellNumber">Cell number to check.</param>
    /// <param name="code">OTP code.</param>
    /// <param name="purpose">Purpose of the OTP.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>OTP identifier if verified.</returns>
    Task<long?> GetVerifiedOtpIdAsync(
        string cellNumber,
        string code,
        string purpose,
        CancellationToken cancellationToken = default);
}
