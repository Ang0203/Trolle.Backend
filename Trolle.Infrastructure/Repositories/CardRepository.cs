using Microsoft.EntityFrameworkCore;
using Trolle.Application.Interfaces.Persistence;
using Trolle.Domain.Entities;
using Trolle.Infrastructure.Persistence;

namespace Trolle.Infrastructure.Repositories;

/// <summary>
/// Repository for managing card persistence.
/// </summary>
public class CardRepository : ICardRepository
{
    private readonly TrolleDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CardRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CardRepository(TrolleDbContext context)
    {
        _context = context;
    }

    #region CRUD Operations

    /// <inheritdoc />
    public async Task<Card?> GetByIdAsync(Guid id)
    {
        return await _context.Cards
            .Include(c => c.Labels)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <inheritdoc />
    public async Task AddAsync(Card card)
    {
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        var card = await _context.Cards.FindAsync(id);
        if (card != null)
        {
            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Card card)
    {
        _context.Cards.Update(card);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new Trolle.Application.Common.Exceptions.ConcurrencyException(
                "The card was modified by another user.", ex);
        }
    }

    #endregion
}
