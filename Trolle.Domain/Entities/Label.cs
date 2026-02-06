using Trolle.Domain.Common;

namespace Trolle.Domain.Entities;

/// <summary>
/// Represents a label that can be assigned to cards.
/// </summary>
public class Label : BaseEntity
{
    #region Fields

    private readonly List<Card> _cards = new();

    #endregion

    #region Properties

    /// <summary>
    /// The name of the label.
    /// </summary>
    public Title Name { get; private set; }

    /// <summary>
    /// The text color for the label.
    /// </summary>
    public CssColor TextColor { get; private set; }

    /// <summary>
    /// The background color of the label.
    /// </summary>
    public CssColor Color { get; private set; }

    /// <summary>
    /// The ID of the board this label belongs to.
    /// </summary>
    public Guid BoardId { get; private set; }

    /// <summary>
    /// Gets the list of cards this label is assigned to.
    /// </summary>
    public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();

    #endregion

    #region Constructors

    private Label()
    {
        Name = null!;
        Color = null!;
        TextColor = null!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Label"/> class.
    /// </summary>
    public Label(string name, string color, string textColor, Guid boardId)
    {
        Name = Title.Create(name);
        TextColor = CssColor.Create(textColor);
        Color = CssColor.Create(color);
        BoardId = boardId;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Updates label properties.
    /// </summary>
    /// <param name="name">The new name of the label.</param>
    /// <param name="color">The new background color.</param>
    /// <param name="textColor">The new text color.</param>
    public void Update(string name, string color, string textColor)
    {
        Name = Title.Create(name);
        Color = CssColor.Create(color);
        TextColor = CssColor.Create(textColor);
        UpdateAudit();
    }

    #endregion
}
