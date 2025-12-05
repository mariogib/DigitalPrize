using DigitalPrizes.Api.Data;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrizes.Api.Repositories.Implementations;

/// <summary>
/// EF Core implementation of IOtpRepository.
/// </summary>
public class OtpRepository : RepositoryBase<Otp, long>, IOtpRepository
{
    public OtpRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Otp?> GetLatestValidAsync(
        string cellNumber,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(o => o.CellNumber == cellNumber &&
                        o.Purpose == purpose &&
                        !o.IsUsed &&
                        o.ExpiresAt > now &&
                        o.AttemptCount < o.MaxAttempts)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Otp?> GetByCodeAsync(
        string cellNumber,
        string code,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.CellNumber == cellNumber &&
                        o.Code == code &&
                        o.Purpose == purpose)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task InvalidateExistingAsync(
        string cellNumber,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        await Context.Otps
            .Where(o => o.CellNumber == cellNumber &&
                        o.Purpose == purpose &&
                        !o.IsUsed &&
                        o.ExpiresAt > now)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(o => o.IsUsed, true)
                    .SetProperty(o => o.UsedAt, now),
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> IncrementAttemptCountAsync(
        long otpId,
        CancellationToken cancellationToken = default)
    {
        var otp = await DbSet.FindAsync(new object[] { otpId }, cancellationToken);
        if (otp == null)
        {
            return -1;
        }

        otp.AttemptCount++;
        await SaveChangesAsync(cancellationToken);
        return otp.MaxAttempts - otp.AttemptCount;
    }

    /// <inheritdoc />
    public async Task MarkAsUsedAsync(
        long otpId,
        CancellationToken cancellationToken = default)
    {
        await Context.Otps
            .Where(o => o.OtpId == otpId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(o => o.IsUsed, true)
                    .SetProperty(o => o.UsedAt, DateTime.UtcNow),
                cancellationToken);
    }
}
