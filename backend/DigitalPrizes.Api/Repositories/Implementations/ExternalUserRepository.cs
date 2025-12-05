using DigitalPrizes.Api.Data;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrizes.Api.Repositories.Implementations;

/// <summary>
/// EF Core implementation of IExternalUserRepository.
/// </summary>
public class ExternalUserRepository : RepositoryBase<ExternalUser, long>, IExternalUserRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalUserRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ExternalUserRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<ExternalUser?> GetByCellNumberAsync(string cellNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.CellNumber == cellNumber, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ExternalUser?> GetWithAllRelationsAsync(long externalUserId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.Registrations)
            .ThenInclude(r => r.Competition)
            .Include(u => u.PrizeAwards)
            .ThenInclude(a => a.Prize)
            .ThenInclude(p => p.PrizeType)
            .FirstOrDefaultAsync(u => u.ExternalUserId == externalUserId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ExternalUser> GetOrCreateAsync(
        string cellNumber,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        CancellationToken cancellationToken = default)
    {
        var user = await GetByCellNumberAsync(cellNumber, cancellationToken);

        if (user != null)
        {
            // Update user info if provided and not already set
            var updated = false;
            if (!string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(user.FirstName))
            {
                user.FirstName = firstName;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(user.LastName))
            {
                user.LastName = lastName;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(user.Email))
            {
                user.Email = email;
                updated = true;
            }

            if (updated)
            {
                await UpdateAsync(user, cancellationToken);
            }

            return user;
        }

        var newUser = new ExternalUser
        {
            CellNumber = cellNumber,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            CreatedAt = DateTime.UtcNow,
        };

        return await AddAsync(newUser, cancellationToken);
    }
}
