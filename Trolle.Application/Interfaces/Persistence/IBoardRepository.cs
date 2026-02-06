using Trolle.Domain.Entities;

namespace Trolle.Application.Interfaces.Persistence;

/// <summary>
/// Provides data access operations for boards.
/// </summary>
public interface IBoardRepository
{
    /// <summary>
    /// Gets all boards.
    /// </summary>
    Task<IEnumerable<Board>> GetAllAsync();

    /// <summary>
    /// Gets a board by its identifier.
    /// </summary>
    Task<Board?> GetByIdAsync(Guid id);

    /// <summary>
    /// Adds a new board.
    /// </summary>
    Task AddAsync(Board board);

    /// <summary>
    /// Deletes a board by its identifier.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Updates an existing board.
    /// </summary>
    Task UpdateAsync(Board board);

    /// <summary>
    /// Gets a board with its columns, cards, labels by its identifier.
    /// </summary>
    Task<Board?> GetByIdWithDetailsAsync(Guid id);
}