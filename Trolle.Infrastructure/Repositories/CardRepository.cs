using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trolle.Application.Interfaces.Persistence;
using Trolle.Domain.Entities;
using Trolle.Infrastructure.Persistence;

namespace Trolle.Infrastructure.Repositories;

public class CardRepository : ICardRepository
{
    private readonly TrolleDbContext _context;

    public CardRepository(TrolleDbContext context)
    {
        _context = context;
    }

    public async Task<Card?> GetByIdAsync(Guid id)
    {
        return await _context.Cards
            .Include(c => c.Labels)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Card card)
    {
        await _context.Cards.AddAsync(card);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Card card)
    {
        _context.Cards.Update(card);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var card = await _context.Cards.FindAsync(id);
        if (card != null)
        {
            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();
        }
    }
}
