using Trolle.Domain.Entities;

namespace Trolle.Application.Interfaces.Persistence;

/// <summary>
/// Provides data access operations for labels.
/// </summary>
public interface ILabelRepository
{
    /// <summary>
    /// Gets a label by its identifier.
    /// </summary>
    Task<Label?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets multiple labels by their identifiers.
    /// </summary>
    Task<IEnumerable<Label>> GetByIdsAsync(IEnumerable<Guid> ids);

    /// <summary>
    /// Adds a new label.
    /// </summary>
    Task AddAsync(Label label);

    /// <summary>
    /// Deletes a label by its identifier.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Updates an existing label.
    /// </summary>
    Task UpdateAsync(Label label);
}