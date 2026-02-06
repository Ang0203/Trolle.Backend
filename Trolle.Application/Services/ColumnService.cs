using Trolle.Application.Interfaces;
using Trolle.Application.Interfaces.Persistence;
using Trolle.Domain.Entities;

namespace Trolle.Application.Services;

/// <summary>
/// Service for managing columns.
/// </summary>
public class ColumnService : IColumnService
{
    private readonly IBoardRepository _boardRepo;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnService"/> class.
    /// </summary>
    /// <param name="boardRepo">The board repository.</param>
    public ColumnService(IBoardRepository boardRepo)
    {
        _boardRepo = boardRepo;
    }

    #region Column Management

    /// <inheritdoc />
    public async Task CreateColumnAsync(Guid boardId, string title, string? headerColor = null)
    {
        // Validate and sanitize inputs
        title = Application.Common.InputValidator.ValidateTitle(title, "New Column");
        headerColor = Application.Common.InputValidator.ValidateColor(headerColor, "transparent");
        
        var board = await _boardRepo.GetByIdWithDetailsAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        var maxOrder = board.Columns.Any() ? board.Columns.Max(c => c.Order) : 0;
        board.AddColumn(title, maxOrder + 1, headerColor);
        
        await _boardRepo.UpdateAsync(board);
    }

    /// <inheritdoc />
    public async Task UpdateColumnAsync(Guid boardId, Guid columnId, string title, string titleColor, string headerColor)
    {
        // Validate and sanitize inputs
        title = Application.Common.InputValidator.ValidateTitle(title, "New Column");
        titleColor = Application.Common.InputValidator.ValidateColor(titleColor);
        headerColor = Application.Common.InputValidator.ValidateColor(headerColor, "transparent");
        
        var board = await _boardRepo.GetByIdWithDetailsAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        var column = board.Columns.FirstOrDefault(c => c.Id == columnId);
        if (column == null) throw new Exception("Column not found");

        column.UpdateTitle(title);
        column.UpdateTitleColor(titleColor);
        column.UpdateHeaderColor(headerColor);
        
        await _boardRepo.UpdateAsync(board);
    }

    /// <inheritdoc />
    public async Task DeleteColumnAsync(Guid boardId, Guid columnId)
    {
        var board = await _boardRepo.GetByIdWithDetailsAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        board.RemoveColumn(columnId);
        await _boardRepo.UpdateAsync(board);
    }

    #endregion

    #region Column Movement

    /// <inheritdoc />
    public async Task MoveColumnAsync(Guid boardId, Guid columnId, int newOrder)
    {
        // Validate order
        newOrder = Application.Common.InputValidator.ValidateOrder(newOrder);
        
        var board = await _boardRepo.GetByIdWithDetailsAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        var columns = board.Columns.OrderBy(c => c.Order).ToList();
        var columnToMove = columns.FirstOrDefault(c => c.Id == columnId);
        if (columnToMove == null) throw new Exception("Column not found");

        columns.Remove(columnToMove);
        if (newOrder < 0) newOrder = 0;
        if (newOrder > columns.Count) newOrder = columns.Count;
        
        columns.Insert(newOrder, columnToMove);

        // Update all orders
        for (int i = 0; i < columns.Count; i++)
        {
            columns[i].SetOrder(i);
        }
        
        await _boardRepo.UpdateAsync(board);
    }

    /// <inheritdoc />
    public async Task BulkMoveColumnsAsync(Guid boardId, Dictionary<Guid, int> columnOrders)
    {
        // Validate bulk operation count
        Application.Common.InputValidator.ValidateBulkOperationCount(columnOrders.Count);
        
        var board = await _boardRepo.GetByIdWithDetailsAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        foreach (var column in board.Columns)
        {
            if (columnOrders.TryGetValue(column.Id, out int newOrder))
            {
                // Validate order
                newOrder = Application.Common.InputValidator.ValidateOrder(newOrder);
                column.SetOrder(newOrder);
            }
        }

        await _boardRepo.UpdateAsync(board);
    }

    #endregion
}
