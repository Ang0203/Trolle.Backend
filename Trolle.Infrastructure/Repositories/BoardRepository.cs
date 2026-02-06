using Microsoft.EntityFrameworkCore;
using Trolle.Application.Interfaces.Persistence;
using Trolle.Domain.Entities;
using Trolle.Infrastructure.Persistence;

namespace Trolle.Infrastructure.Repositories;

/// <summary>
/// Repository for managing board persistence.
/// </summary>
public class BoardRepository : IBoardRepository
{
    private readonly TrolleDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoardRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public BoardRepository(TrolleDbContext context)
    {
        _context = context;
    }

    #region CRUD Operations

    /// <inheritdoc />
    public async Task<IEnumerable<Board>> GetAllAsync()
    {
        return await _context.Boards.ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Board?> GetByIdAsync(Guid id)
    {
        return await _context.Boards.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task AddAsync(Board board)
    {
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        var board = await _context.Boards.FindAsync(id);
        if (board != null)
        {
            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Board board)
    {
        _context.Boards.Update(board);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new Trolle.Application.Common.Exceptions.ConcurrencyException(
                "The board was modified by another user.", ex);
        }
    }

    #endregion

    #region Specialized Queries

    /// <inheritdoc />
    public async Task<Board?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Boards
            .Include(b => b.Labels)
            .Include(b => b.Columns)
                .ThenInclude(c => c.Cards)
                    .ThenInclude(card => card.Labels)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    #endregion
}