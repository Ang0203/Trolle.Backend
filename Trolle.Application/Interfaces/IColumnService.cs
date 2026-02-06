namespace Trolle.Application.Interfaces;

/// <summary>
/// Service for managing columns.
/// </summary>
public interface IColumnService
{
    /// <summary>
    /// Creates a new column in a board.
    /// </summary>
    Task CreateColumnAsync(Guid boardId, string title, string? headerColor = null);

    /// <summary>
    /// Deletes a column.
    /// </summary>
    Task DeleteColumnAsync(Guid boardId, Guid columnId);

    /// <summary>
    /// Updates an existing column.
    /// </summary>
    Task UpdateColumnAsync(Guid boardId, Guid columnId, string title, string titleColor, string headerColor);

    /// <summary>
    /// Moves a column to a new position.
    /// </summary>
    Task MoveColumnAsync(Guid boardId, Guid columnId, int newOrder);

    /// <summary>
    /// Performs a bulk move of columns.
    /// </summary>
    Task BulkMoveColumnsAsync(Guid boardId, Dictionary<Guid, int> columnOrders);
}