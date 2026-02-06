using Microsoft.EntityFrameworkCore;
using Trolle.Application.Interfaces.Persistence;
using Trolle.Domain.Entities;
using Trolle.Infrastructure.Persistence;

namespace Trolle.Infrastructure.Repositories;

/// <summary>
/// Repository for managing label persistence.
/// </summary>
public class LabelRepository : ILabelRepository
{
    private readonly TrolleDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="LabelRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public LabelRepository(TrolleDbContext context)
    {
        _context = context;
    }

    #region CRUD Operations

    /// <inheritdoc />
    public async Task<Label?> GetByIdAsync(Guid id)
    {
        return await _context.Labels.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Label>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _context.Labels
            .Where(l => ids.Contains(l.Id))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task AddAsync(Label label)
    {
        _context.Labels.Add(label);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        var label = await _context.Labels.FindAsync(id);
        if (label != null)
        {
            _context.Labels.Remove(label);
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Label label)
    {
        _context.Labels.Update(label);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new Trolle.Application.Common.Exceptions.ConcurrencyException(
                "The label was modified by another user.", ex);
        }
    }

    #endregion
}