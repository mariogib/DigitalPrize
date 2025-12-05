using DigitalPrizes.Api.Data;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrizes.Api.Repositories.Implementations;

/// <summary>
/// EF Core implementation of IRegistrationRepository.
/// </summary>
public class RegistrationRepository : RepositoryBase<Registration, long>, IRegistrationRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public RegistrationRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<PagedResponse<Registration>> GetPagedAsync(
        FilterParameters parameters,
        int? competitionId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var query = DbSet
            .Include(r => r.Competition)
            .Include(r => r.ExternalUser)
            .AsQueryable();

        if (competitionId.HasValue)
        {
            query = query.Where(r => r.CompetitionId == competitionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm;
            query = query.Where(r => r.CellNumber.Contains(searchTerm) ||
                                     (r.ExternalUser != null && (
                                         (r.ExternalUser.FirstName != null && EF.Functions.Like(r.ExternalUser.FirstName, $"%{searchTerm}%")) ||
                                         (r.ExternalUser.LastName != null && EF.Functions.Like(r.ExternalUser.LastName, $"%{searchTerm}%")))));
        }

        query = parameters.SortBy?.ToUpperInvariant() switch
        {
            "CELLNUMBER" => parameters.SortDescending ? query.OrderByDescending(r => r.CellNumber) : query.OrderBy(r => r.CellNumber),
            _ => query.OrderByDescending(r => r.RegistrationDate),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<Registration>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
        };
    }

    /// <inheritdoc />
    public async Task<Registration?> GetByCompetitionAndCellAsync(
        int competitionId,
        string cellNumber,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.RegistrationAnswers)
            .ThenInclude(a => a.RegistrationField)
            .FirstOrDefaultAsync(r => r.CompetitionId == competitionId &&
                                      r.CellNumber == cellNumber,
                                 cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Registration?> GetWithAnswersAsync(long registrationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Competition)
            .Include(r => r.ExternalUser)
            .Include(r => r.RegistrationAnswers)
            .ThenInclude(a => a.RegistrationField)
            .FirstOrDefaultAsync(r => r.RegistrationId == registrationId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Registration>> GetByCompetitionAsync(
        int competitionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.ExternalUser)
            .Where(r => r.CompetitionId == competitionId)
            .OrderByDescending(r => r.RegistrationDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Registration>> GetByExternalUserAsync(
        long externalUserId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Competition)
            .Where(r => r.ExternalUserId == externalUserId)
            .OrderByDescending(r => r.RegistrationDate)
            .ToListAsync(cancellationToken);
    }
}
