using Trolle.Application.Interfaces;
using Trolle.Application.Interfaces.Persistence;
using Trolle.Domain.Entities;

namespace Trolle.Application.Services;

/// <summary>
/// Service for managing cards.
/// </summary>
public class CardService : ICardService
{
    private readonly IBoardRepository _boardRepo;
    private readonly ICardRepository _cardRepo;
    private readonly ILabelRepository _labelRepo;

    /// <summary>
    /// Initializes a new instance of the <see cref="CardService"/> class.
    /// </summary>
    /// <param name="boardRepo">The board repository.</param>
    /// <param name="cardRepo">The card repository.</param>
    /// <param name="labelRepo">The label repository.</param>
    public CardService(IBoardRepository boardRepo, ICardRepository cardRepo, ILabelRepository labelRepo)
    {
        _boardRepo = boardRepo;
        _cardRepo = cardRepo;
        _labelRepo = labelRepo;
    }

    #region Card Management

    /// <inheritdoc />
    public async Task CreateCardAsync(Guid columnId, string title, string? description, List<Guid> labelIds)
    {
        // Validate and sanitize inputs
        title = Application.Common.InputValidator.ValidateTitle(title, "New Card");
        description = Application.Common.InputValidator.ValidateDescription(description);
        
        var card = new Card(title, description, 0, columnId);
        if (labelIds != null && labelIds.Any())
        {
            var labels = await _labelRepo.GetByIdsAsync(labelIds);
            card.SetLabels(labels);
        }
        await _cardRepo.AddAsync(card);
    }

    /// <inheritdoc />
    public async Task UpdateCardAsync(Guid cardId, string title, string? description, List<Guid> labelIds)
    {
        // Validate and sanitize inputs
        title = Application.Common.InputValidator.ValidateTitle(title, "New Card");
        description = Application.Common.InputValidator.ValidateDescription(description);
        
        var card = await _cardRepo.GetByIdAsync(cardId);
        if (card == null) throw new Exception("Card not found");

        card.Update(title, description);
        
        if (labelIds != null)
        {
            var labels = await _labelRepo.GetByIdsAsync(labelIds);
            card.SetLabels(labels);
        }
        
        await _cardRepo.UpdateAsync(card);
    }

    /// <inheritdoc />
    public async Task DeleteCardAsync(Guid cardId)
    {
        await _cardRepo.DeleteAsync(cardId);
    }

    #endregion

    #region Archiving

    /// <inheritdoc />
    public async Task ArchiveCardAsync(Guid cardId)
    {
        var card = await _cardRepo.GetByIdAsync(cardId);
        if (card == null) throw new Exception("Card not found");
        card.Archive();
        await _cardRepo.UpdateAsync(card);
    }

    /// <inheritdoc />
    public async Task UnarchiveCardAsync(Guid cardId)
    {
        var card = await _cardRepo.GetByIdAsync(cardId);
        if (card == null) throw new Exception("Card not found");
        card.Unarchive();
        await _cardRepo.UpdateAsync(card);
    }

    #endregion

    #region Card Movement

    /// <inheritdoc />
    public async Task MoveCardAsync(Guid cardId, Guid targetColumnId, int newOrder)
    {
        // Validate order
        newOrder = Application.Common.InputValidator.ValidateOrder(newOrder);
        
        var card = await _cardRepo.GetByIdAsync(cardId);
        if (card == null) throw new Exception("Card not found");

        card.MoveToColumn(targetColumnId);
        card.SetOrder(newOrder);
        
        await _cardRepo.UpdateAsync(card);
    }

    /// <inheritdoc />
    public async Task BulkMoveCardsAsync(Guid boardId, Dictionary<Guid, int> cardOrders)
    {
        // Validate bulk operation count
        Application.Common.InputValidator.ValidateBulkOperationCount(cardOrders.Count);
        
        var board = await _boardRepo.GetByIdWithDetailsAsync(boardId);
        if (board == null) throw new Exception("Board not found");

        var allCards = board.Columns.SelectMany(c => c.Cards).ToList();

        foreach (var card in allCards)
        {
            if (cardOrders.TryGetValue(card.Id, out int newOrder))
            {
                // Validate order
                newOrder = Application.Common.InputValidator.ValidateOrder(newOrder);
                card.SetOrder(newOrder);
            }
        }

        await _boardRepo.UpdateAsync(board);
    }

    #endregion
}
