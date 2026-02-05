using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trolle.Application.DTOs;
using Trolle.Application.Interfaces;
using Trolle.Application.Interfaces.Persistence;
using Trolle.Domain.Entities;

namespace Trolle.Application.Services;

public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepo;
    private readonly ICardRepository _cardRepo;
    private readonly ILabelRepository _labelRepo;

    public BoardService(IBoardRepository boardRepo, ICardRepository cardRepo, ILabelRepository labelRepo)
    {
        _boardRepo = boardRepo;
        _cardRepo = cardRepo;
        _labelRepo = labelRepo;
    }

    public async Task<IEnumerable<BoardDto>> GetBoardsAsync()
    {
        var boards = await _boardRepo.GetAllAsync();
        // Manual mapping for now
        return boards
            .OrderByDescending(b => b.IsFavorite)
            .ThenByDescending(b => b.LastModifiedAt)
            .Select(b => new BoardDto
        {
            Id = b.Id,
            Title = b.Title,
            BackgroundImage = b.BackgroundImage,
            TitleColor = b.TitleColor,
            BackgroundColor = b.BackgroundColor,
            IsFavorite = b.IsFavorite,
            CreatedAt = b.CreatedAt
        });
    }

    public async Task<BoardDto?> GetBoardAsync(Guid id)
    {
        var board = await _boardRepo.GetByIdWithDetailsAsync(id);
        if (board == null) return null;

        return new BoardDto
        {
            Id = board.Id,
            Title = board.Title,
            BackgroundImage = board.BackgroundImage,
            TitleColor = board.TitleColor,
            BackgroundColor = board.BackgroundColor,
            IsFavorite = board.IsFavorite,
            CreatedAt = board.CreatedAt,
            Labels = board.Labels.Select(l => new LabelDto
            {
                Id = l.Id,
                Name = l.Name,
                Color = l.Color,
                TextColor = l.TextColor,
                BoardId = l.BoardId
            }).ToList(),
            Columns = board.Columns.OrderBy(c => c.Order).Select(c => new ColumnDto
            {
                Id = c.Id,
                Title = c.Title,
                TitleColor = c.TitleColor,
                HeaderColor = c.HeaderColor,
                Order = c.Order,
                Cards = c.Cards.OrderBy(card => card.Order).Select(card => new CardDto
                {
                    Id = card.Id,
                    Title = card.Title,
                    Description = card.Description,
                    Order = card.Order,
                    ColumnId = card.ColumnId,
                    IsArchived = card.IsArchived,
                    Labels = card.Labels.Select(l => new LabelDto
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Color = l.Color,
                        TextColor = l.TextColor,
                        BoardId = l.BoardId
                    }).ToList()
                }).ToList()
            }).ToList()
        };
    }

    public async Task<Guid> CreateBoardAsync(string title, string? backgroundImage = null)
    {
        // Default to "New Board" if empty
        if (string.IsNullOrWhiteSpace(title)) title = "New Board";

        var board = new Board(title, backgroundImage);
        await _boardRepo.AddAsync(board);
        return board.Id;
    }

    public async Task UpdateTitleAsync(Guid boardId, string newTitle)
    {
        var board = await _boardRepo.GetByIdAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        board.UpdateTitle(newTitle);
        await _boardRepo.UpdateAsync(board);
    }

    public async Task UpdateBoardTitleColorAsync(Guid boardId, string color)
    {
        var board = await _boardRepo.GetByIdAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        board.UpdateTitleColor(color);
        await _boardRepo.UpdateAsync(board);
    }

    public async Task UpdateBoardBackgroundColorAsync(Guid boardId, string color)
    {
        var board = await _boardRepo.GetByIdAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        board.UpdateBackgroundColor(color);
        await _boardRepo.UpdateAsync(board);
    }

    public async Task ToggleFavoriteAsync(Guid boardId)
    {
        var board = await _boardRepo.GetByIdAsync(boardId);
        if (board == null) throw new Exception("Board not found");
        
        board.ToggleFavorite();
        await _boardRepo.UpdateAsync(board);
    }

    public async Task DeleteBoardAsync(Guid id)
    {
        await _boardRepo.DeleteAsync(id);
    }

    public async Task CreateColumnAsync(Guid boardId, string title, string? headerColor = null)
    {
        var board = await _boardRepo.GetByIdWithDetailsAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        var maxOrder = board.Columns.Any() ? board.Columns.Max(c => c.Order) : 0;
        board.AddColumn(title, maxOrder + 1, headerColor ?? "transparent");
        
        await _boardRepo.UpdateAsync(board);
    }

    public async Task UpdateColumnAsync(Guid boardId, Guid columnId, string title, string titleColor, string headerColor)
    {
        var board = await _boardRepo.GetByIdWithDetailsAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        var column = board.Columns.FirstOrDefault(c => c.Id == columnId);
        if (column == null) throw new Exception("Column not found");

        column.UpdateTitle(title);
        column.UpdateTitleColor(titleColor);
        column.UpdateHeaderColor(headerColor);
        
        await _boardRepo.UpdateAsync(board);
    }

    public async Task DeleteColumnAsync(Guid boardId, Guid columnId)
    {
        var board = await _boardRepo.GetByIdWithDetailsAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        board.RemoveColumn(columnId);
        await _boardRepo.UpdateAsync(board);
    }

    public async Task CreateCardAsync(Guid columnId, string title, string? description, List<Guid> labelIds)
    {
        var card = new Card(title, description ?? string.Empty, 0, columnId);
        if (labelIds != null && labelIds.Any())
        {
            var labels = await _labelRepo.GetByIdsAsync(labelIds);
            card.SetLabels(labels);
        }
        await _cardRepo.AddAsync(card);
    }

    public async Task UpdateCardAsync(Guid cardId, string title, string? description, List<Guid> labelIds)
    {
        var card = await _cardRepo.GetByIdAsync(cardId);
        if (card == null) throw new Exception("Card not found");

        card.Update(title, description ?? string.Empty);
        
        if (labelIds != null)
        {
            var labels = await _labelRepo.GetByIdsAsync(labelIds);
            card.SetLabels(labels);
        }
        
        await _cardRepo.UpdateAsync(card);
    }

    public async Task DeleteCardAsync(Guid cardId)
    {
        await _cardRepo.DeleteAsync(cardId);
    }

    public async Task ArchiveCardAsync(Guid cardId)
    {
        var card = await _cardRepo.GetByIdAsync(cardId);
        if (card == null) throw new Exception("Card not found");
        card.Archive();
        await _cardRepo.UpdateAsync(card);
    }

    public async Task UnarchiveCardAsync(Guid cardId)
    {
        var card = await _cardRepo.GetByIdAsync(cardId);
        if (card == null) throw new Exception("Card not found");
        card.Unarchive();
        await _cardRepo.UpdateAsync(card);
    }

    public async Task<Guid> CreateLabelAsync(Guid boardId, string name, string color, string textColor)
    {
        var label = new Label(name, color, textColor, boardId);
        await _labelRepo.AddAsync(label);
        return label.Id;
    }

    public async Task UpdateLabelAsync(Guid labelId, string name, string color, string textColor)
    {
        var label = await _labelRepo.GetByIdAsync(labelId);
        if (label == null) throw new Exception("Label not found");

        label.Update(name, color, textColor);
        await _labelRepo.UpdateAsync(label);
    }

    public async Task DeleteLabelAsync(Guid labelId)
    {
        await _labelRepo.DeleteAsync(labelId);
    }

    public async Task MoveCardAsync(Guid cardId, Guid targetColumnId, int newOrder)
    {
        var card = await _cardRepo.GetByIdAsync(cardId);
        if (card == null) throw new Exception("Card not found");

        card.MoveToColumn(targetColumnId);
        card.SetOrder(newOrder);
        
        await _cardRepo.UpdateAsync(card);
    }

    public async Task MoveColumnAsync(Guid boardId, Guid columnId, int newOrder)
    {
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

    public async Task BulkMoveColumnsAsync(Guid boardId, Dictionary<Guid, int> columnOrders)
    {
        var board = await _boardRepo.GetByIdWithDetailsAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        foreach (var column in board.Columns)
        {
            if (columnOrders.TryGetValue(column.Id, out int newOrder))
            {
                column.SetOrder(newOrder);
            }
        }

        await _boardRepo.UpdateAsync(board);
    }

    public async Task BulkMoveCardsAsync(Guid boardId, Dictionary<Guid, int> cardOrders)
    {
        var board = await _boardRepo.GetByIdWithDetailsAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        var allCards = board.Columns.SelectMany(c => c.Cards).ToList();

        foreach (var card in allCards)
        {
            if (cardOrders.TryGetValue(card.Id, out int newOrder))
            {
                card.SetOrder(newOrder);
            }
        }

        await _boardRepo.UpdateAsync(board);
    }
}
