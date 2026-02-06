using Trolle.Application.Interfaces;
using Trolle.Application.Interfaces.Persistence;
using Trolle.Domain.Entities;

namespace Trolle.Application.Services;

/// <summary>
/// Service for managing labels.
/// </summary>
public class LabelService : ILabelService
{
    private readonly ILabelRepository _labelRepo;

    /// <summary>
    /// Initializes a new instance of the <see cref="LabelService"/> class.
    /// </summary>
    /// <param name="labelRepo">The label repository.</param>
    public LabelService(ILabelRepository labelRepo)
    {
        _labelRepo = labelRepo;
    }

    #region Label Management

    /// <inheritdoc />
    public async Task<Guid> CreateLabelAsync(Guid boardId, string name, string color, string textColor)
    {
        // Validate and sanitize inputs
        name = Application.Common.InputValidator.ValidateTitle(name, "New Label");
        color = Application.Common.InputValidator.ValidateColor(color);
        textColor = Application.Common.InputValidator.ValidateColor(textColor, "#ffffff");
        
        var label = new Label(name, color, textColor, boardId);
        await _labelRepo.AddAsync(label);
        return label.Id;
    }

    /// <inheritdoc />
    public async Task UpdateLabelAsync(Guid labelId, string name, string color, string textColor)
    {
        // Validate and sanitize inputs
        name = Application.Common.InputValidator.ValidateTitle(name, "New Label");
        color = Application.Common.InputValidator.ValidateColor(color);
        textColor = Application.Common.InputValidator.ValidateColor(textColor, "#ffffff");
        
        var label = await _labelRepo.GetByIdAsync(labelId);
        if (label == null) throw new Exception("Label not found");

        label.Update(name, color, textColor);
        await _labelRepo.UpdateAsync(label);
    }

    /// <inheritdoc />
    public async Task DeleteLabelAsync(Guid labelId)
    {
        await _labelRepo.DeleteAsync(labelId);
    }

    #endregion
}
