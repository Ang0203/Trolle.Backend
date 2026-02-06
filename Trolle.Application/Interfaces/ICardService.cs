namespace Trolle.Application.Interfaces;

/// <summary>
/// Service for managing cards.
/// </summary>
public interface ICardService
{
    /// <summary>
    /// Creates a new card in a column.
    /// </summary>
    Task CreateCardAsync(Guid columnId, string title, string? description, List<Guid> labelIds);

    /// <summary>
    /// Deletes a card.
    /// </summary>
    Task DeleteCardAsync(Guid cardId);

    /// <summary>
    /// Updates an existing card.
    /// </summary>
    Task UpdateCardAsync(Guid cardId, string title, string? description, List<Guid> labelIds);

    /// <summary>
    /// Archives a card.
    /// </summary>
    Task ArchiveCardAsync(Guid cardId);

    /// <summary>
    /// Unarchives a card.
    /// </summary>
    Task UnarchiveCardAsync(Guid cardId);

    /// <summary>
    /// Moves a card to a new column or position.
    /// </summary>
    Task MoveCardAsync(Guid cardId, Guid targetColumnId, int newOrder);

    /// <summary>
    /// Performs a bulk move of cards.
    /// </summary>
    Task BulkMoveCardsAsync(Guid boardId, Dictionary<Guid, int> cardOrders);
}