using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trolle.Application.DTOs;

namespace Trolle.Application.Interfaces;

public interface IBoardService
{
    Task<IEnumerable<BoardDto>> GetBoardsAsync();
    Task<BoardDto?> GetBoardAsync(Guid id);
    Task<Guid> CreateBoardAsync(string title, string? backgroundImage = null);
    Task UpdateTitleAsync(Guid boardId, string newTitle);
    Task UpdateBoardTitleColorAsync(Guid boardId, string color);
    Task UpdateBoardBackgroundColorAsync(Guid boardId, string color);
    Task ToggleFavoriteAsync(Guid boardId);
    Task DeleteBoardAsync(Guid id);
    
    // Board management
    Task CreateColumnAsync(Guid boardId, string title, string? headerColor = null);
    Task UpdateColumnAsync(Guid boardId, Guid columnId, string title, string titleColor, string headerColor);
    Task DeleteColumnAsync(Guid boardId, Guid columnId);
    Task CreateCardAsync(Guid columnId, string title, string? description, List<Guid> labelIds);
    Task UpdateCardAsync(Guid cardId, string title, string? description, List<Guid> labelIds);
    Task DeleteCardAsync(Guid cardId);
    Task ArchiveCardAsync(Guid cardId);
    Task UnarchiveCardAsync(Guid cardId);
    
    // Label management
    Task<Guid> CreateLabelAsync(Guid boardId, string name, string color, string textColor);
    Task UpdateLabelAsync(Guid labelId, string name, string color, string textColor);
    Task DeleteLabelAsync(Guid labelId);

    // Drag and Drop actions
    Task MoveCardAsync(Guid cardId, Guid targetColumnId, int newOrder);
    Task MoveColumnAsync(Guid boardId, Guid columnId, int newOrder);
    Task BulkMoveColumnsAsync(Guid boardId, Dictionary<Guid, int> columnOrders);
    Task BulkMoveCardsAsync(Guid boardId, Dictionary<Guid, int> cardOrders);
}
