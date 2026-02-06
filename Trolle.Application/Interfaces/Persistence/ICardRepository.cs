using Trolle.Domain.Entities;

namespace Trolle.Application.Interfaces.Persistence;

/// <summary>
/// Provides data access operations for cards.
/// </summary>
public interface ICardRepository
{
    /// <summary>
    /// Gets a card by its identifier.
    /// </summary>
    Task<Card?> GetByIdAsync(Guid id);

    /// <summary>
    /// Adds a new card.
    /// </summary>
    Task AddAsync(Card card);

    /// <summary>
    /// Deletes a card by its identifier.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Updates an existing card.
    /// </summary>
    Task UpdateAsync(Card card);
}