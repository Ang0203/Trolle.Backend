using Trolle.Application.DTOs;

namespace Trolle.Application.Interfaces;

/// <summary>
/// Service for managing boards.
/// </summary>
public interface IBoardService
{
    /// <summary>
    /// Gets all boards.
    /// </summary>
    Task<IEnumerable<BoardDto>> GetBoardsAsync();

    /// <summary>
    /// Gets a specific board by ID with its columns and cards.
    /// </summary>
    Task<BoardDto?> GetBoardAsync(Guid id);

    /// <summary>
    /// Toggles the favorite status of a board.
    /// </summary>
    Task ToggleFavoriteAsync(Guid boardId);

    /// <summary>
    /// Creates a new board.
    /// </summary>
    Task<Guid> CreateBoardAsync(string title, string? backgroundImage = null);

    /// <summary>
    /// Deletes a board.
    /// </summary>
    Task DeleteBoardAsync(Guid id);

    /// <summary>
    /// Updates the title of a board.
    /// </summary>
    /// <summary>
    /// Updates the title of a board.
    /// </summary>
    Task UpdateTitleAsync(Guid boardId, string newTitle);

    /// <summary>
    /// Updates the title color of a board.
    /// </summary>
    Task UpdateBoardTitleColorAsync(Guid boardId, string color);

    /// <summary>
    /// Updates the background color of a board.
    /// </summary>
    Task UpdateBoardBackgroundColorAsync(Guid boardId, string color);
}