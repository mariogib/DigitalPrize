using DigitalPrizes.Api.Data;
using DigitalPrizes.Api.Models.Domain;
using DigitalPrizes.Api.Models.Dtos.Common;
using DigitalPrizes.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalPrizes.Api.Repositories.Implementations;

/// <summary>
/// EF Core implementation of ICompetitionRepository.
/// </summary>
public class CompetitionRepository : RepositoryBase<Competition, int>, ICompetitionRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompetitionRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CompetitionRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<PagedResponse<Competition>> GetPagedAsync(
        FilterParameters parameters,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            // Convert status filter to IsActive check
            var isActiveFilter = string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase);
            query = query.Where(c => c.IsActive == isActiveFilter);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm;
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{searchTerm}%") ||
                                     (c.Description != null && EF.Functions.Like(c.Description, $"%{searchTerm}%")));
        }

        query = parameters.SortBy?.ToUpperInvariant() switch
        {
            "NAME" => parameters.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "STARTDATE" => parameters.SortDescending ? query.OrderByDescending(c => c.StartDate) : query.OrderBy(c => c.StartDate),
            "ENDDATE" => parameters.SortDescending ? query.OrderByDescending(c => c.EndDate) : query.OrderBy(c => c.EndDate),
            _ => query.OrderByDescending(c => c.CreatedAt),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<Competition>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
        };
    }

    /// <inheritdoc />
    public async Task<Competition?> GetWithFieldsAsync(int competitionId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.RegistrationFields.OrderBy(f => f.DisplayOrder))
            .Include(c => c.Registrations)
            .Include(c => c.PrizePool)
                .ThenInclude(pp => pp!.Prizes)
            .FirstOrDefaultAsync(c => c.CompetitionId == competitionId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Competition?> GetWithAllRelationsAsync(int competitionId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.RegistrationFields.OrderBy(f => f.DisplayOrder))
            .FirstOrDefaultAsync(c => c.CompetitionId == competitionId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Competition>> GetActiveCompetitionsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(c => c.IsActive &&
                        c.StartDate <= now &&
                        c.EndDate >= now)
            .OrderBy(c => c.EndDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RegistrationField?> GetFieldByIdAsync(int registrationFieldId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<RegistrationField>()
            .FirstOrDefaultAsync(f => f.RegistrationFieldId == registrationFieldId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateFieldAsync(RegistrationField field, CancellationToken cancellationToken = default)
    {
        Context.Set<RegistrationField>().Update(field);
        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteFieldAsync(RegistrationField field, CancellationToken cancellationToken = default)
    {
        Context.Set<RegistrationField>().Remove(field);
        await Context.SaveChangesAsync(cancellationToken);
    }
}
