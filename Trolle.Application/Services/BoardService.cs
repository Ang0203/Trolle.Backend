using Trolle.Application.DTOs;
using Trolle.Application.Interfaces;
using Trolle.Application.Interfaces.Persistence;
using Trolle.Domain.Entities;

namespace Trolle.Application.Services;

/// <summary>
/// Service for managing boards.
/// </summary>
public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepo;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoardService"/> class.
    /// </summary>
    /// <param name="boardRepo">The board repository.</param>
    public BoardService(IBoardRepository boardRepo)
    {
        _boardRepo = boardRepo;
    }

    #region Board Queries

    /// <inheritdoc />
    public async Task<IEnumerable<BoardDto>> GetBoardsAsync()
    {
        var boards = await _boardRepo.GetAllAsync();
        return boards
            .OrderByDescending(b => b.IsFavorite)
            .ThenByDescending(b => b.LastModifiedAt)
            .Select(b => new BoardDto
            {
                Id = b.Id,
                Title = b.Title.Value,
                BackgroundImage = b.BackgroundImage,
                TitleColor = b.TitleColor.Value,
                BackgroundColor = b.BackgroundColor.Value,
                IsFavorite = b.IsFavorite,
                CreatedAt = b.CreatedAt,
                RowVersion = b.RowVersion
            });
    }

    /// <inheritdoc />
    public async Task<BoardDto?> GetBoardAsync(Guid id)
    {
        var board = await _boardRepo.GetByIdWithDetailsAsync(id);
        if (board == null) return null;

        var columns = board.Columns.ToList();
        columns.Sort((a, b) => a.Order.CompareTo(b.Order));

        return new BoardDto
        {
            Id = board.Id,
            Title = board.Title.Value,
            BackgroundImage = board.BackgroundImage,
            TitleColor = board.TitleColor.Value,
            BackgroundColor = board.BackgroundColor.Value,
            IsFavorite = board.IsFavorite,
            CreatedAt = board.CreatedAt,
            RowVersion = board.RowVersion,
            Labels = board.Labels.Select(l => new LabelDto
            {
                Id = l.Id,
                Name = l.Name.Value,
                Color = l.Color.Value,
                TextColor = l.TextColor.Value,
                BoardId = l.BoardId,
                RowVersion = l.RowVersion
            }).ToList(),
            Columns = columns.Select(c =>
            {
                var cards = c.Cards.ToList();
                cards.Sort((a, b) => a.Order.CompareTo(b.Order));
                return new ColumnDto
                {
                    Id = c.Id,
                    Title = c.Title.Value,
                    TitleColor = c.TitleColor.Value,
                    HeaderColor = c.HeaderColor.Value,
                    Order = c.Order,
                    RowVersion = c.RowVersion,
                    Cards = cards.Select(card => new CardDto
                    {
                        Id = card.Id,
                        Title = card.Title.Value,
                        Description = card.Description,
                        Order = card.Order,
                        ColumnId = card.ColumnId,
                        IsArchived = card.IsArchived,
                        RowVersion = card.RowVersion,
                        Labels = card.Labels.Select(l => new LabelDto
                        {
                            Id = l.Id,
                            Name = l.Name.Value,
                            Color = l.Color.Value,
                            TextColor = l.TextColor.Value,
                            BoardId = l.BoardId,
                            RowVersion = l.RowVersion
                        }).ToList()
                    }).ToList()
                };
            }).ToList()
        };
    }

    #endregion

    #region Board Management

    /// <inheritdoc />
    public async Task<Guid> CreateBoardAsync(string title, string? backgroundImage = null)
    {
        // Validate and sanitize inputs
        title = Application.Common.InputValidator.ValidateTitle(title, "New Board");
        backgroundImage = Application.Common.InputValidator.ValidateDescription(backgroundImage);

        var board = new Board(title, backgroundImage);
        await _boardRepo.AddAsync(board);
        return board.Id;
    }

    /// <inheritdoc />
    /// <inheritdoc />
    public async Task UpdateTitleAsync(Guid boardId, string newTitle)
    {
        var board = await _boardRepo.GetByIdAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        // Validate and sanitize title
        newTitle = Application.Common.InputValidator.ValidateTitle(newTitle, "New Board");
        
        board.UpdateTitle(newTitle);
        await _boardRepo.UpdateAsync(board);
    }

    /// <inheritdoc />
    /// <inheritdoc />
    public async Task UpdateBoardTitleColorAsync(Guid boardId, string color)
    {
        var board = await _boardRepo.GetByIdAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        // Validate color
        color = Application.Common.InputValidator.ValidateColor(color);
        
        board.UpdateTitleColor(color);
        await _boardRepo.UpdateAsync(board);
    }

    /// <inheritdoc />
    /// <inheritdoc />
    public async Task UpdateBoardBackgroundColorAsync(Guid boardId, string color)
    {
        var board = await _boardRepo.GetByIdAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        // Validate color
        color = Application.Common.InputValidator.ValidateColor(color);
        
        board.UpdateBackgroundColor(color);
        await _boardRepo.UpdateAsync(board);
    }

    /// <inheritdoc />
    public async Task ToggleFavoriteAsync(Guid boardId)
    {
        var board = await _boardRepo.GetByIdAsync(boardId);
        if (board == null) throw new Exception("Board not found");
        
        board.ToggleFavorite();
        await _boardRepo.UpdateAsync(board);
    }

    /// <inheritdoc />
    public async Task DeleteBoardAsync(Guid id)
    {
        await _boardRepo.DeleteAsync(id);
    }

    #endregion
}
