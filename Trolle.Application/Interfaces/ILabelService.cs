namespace Trolle.Application.Interfaces;

/// <summary>
/// Service for managing labels.
/// </summary>
public interface ILabelService
{
    /// <summary>
    /// Creates a new label in a board.
    /// </summary>
    Task<Guid> CreateLabelAsync(Guid boardId, string name, string color, string textColor);

    /// <summary>
    /// Deletes a label.
    /// </summary>
    Task DeleteLabelAsync(Guid labelId);

    /// <summary>
    /// Updates an existing label.
    /// </summary>
    Task UpdateLabelAsync(Guid labelId, string name, string color, string textColor);
}