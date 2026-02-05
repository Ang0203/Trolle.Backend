using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trolle.Application.Interfaces.Persistence;
using Trolle.Domain.Entities;
using Trolle.Infrastructure.Persistence;

namespace Trolle.Infrastructure.Repositories;

public class BoardRepository : IBoardRepository
{
    private readonly TrolleDbContext _context;

    public BoardRepository(TrolleDbContext context)
    {
        _context = context;
    }

    public async Task<Board?> GetByIdAsync(Guid id)
    {
        return await _context.Boards.FindAsync(id);
    }

    public async Task<IEnumerable<Board>> GetAllAsync()
    {
        return await _context.Boards.ToListAsync();
    }

    public async Task AddAsync(Board board)
    {
        await _context.Boards.AddAsync(board);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Board board)
    {
        _context.Boards.Update(board);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var board = await _context.Boards.FindAsync(id);
        if (board != null)
        {
            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Board?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Boards
            .Include(b => b.Labels)
            .Include(b => b.Columns)
                .ThenInclude(c => c.Cards)
                    .ThenInclude(card => card.Labels)
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}
