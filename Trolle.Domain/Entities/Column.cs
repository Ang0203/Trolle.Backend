using Trolle.Domain.Common;

namespace Trolle.Domain.Entities;

/// <summary>
/// Represents a column within a board.
/// </summary>
public class Column : BaseEntity
{
    #region Fields

    private readonly List<Card> _cards = new();

    #endregion

    #region Properties

    /// <summary>
    /// The title of the column.
    /// </summary>
    public Title Title { get; private set; }

    /// <summary>
    /// The title text color.
    /// </summary>
    public CssColor TitleColor { get; private set; }

    /// <summary>
    /// The header background color.
    /// </summary>
    public CssColor HeaderColor { get; private set; }

    /// <summary>
    /// The display order of the column.
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// The ID of the board this column belongs to.
    /// </summary>
    public Guid BoardId { get; private set; }

    /// <summary>
    /// The list of cards in this column.
    /// </summary>
    public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();

    #endregion

    #region Constructors

    private Column()
    {
        Title = null!;
        TitleColor = null!;
        HeaderColor = null!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Column"/> class.
    /// </summary>
    public Column(string title, int order, Guid boardId, string headerColor = "transparent")
    {
        Title = Title.Create(title);
        TitleColor = CssColor.DefaultBoardBackground;
        HeaderColor = CssColor.Create(headerColor);
        Order = order;
        BoardId = boardId;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Updates the column title.
    /// </summary>
    /// <param name="title">The new title.</param>
    public void UpdateTitle(string title)
    {
        Title = Title.Create(title);
        UpdateAudit();
    }

    /// <summary>
    /// Updates the title text color.
    /// </summary>
    /// <param name="color">The new CSS color string.</param>
    public void UpdateTitleColor(string color)
    {
        TitleColor = CssColor.Create(color);
        UpdateAudit();
    }

    /// <summary>
    /// Updates the header background color.
    /// </summary>
    /// <param name="color">The new CSS color string.</param>
    public void UpdateHeaderColor(string color)
    {
        HeaderColor = CssColor.Create(color);
        UpdateAudit();
    }

    /// <summary>
    /// Sets the column order.
    /// </summary>
    /// <param name="order">The new display order.</param>
    public void SetOrder(int order)
    {
        Order = order;
        UpdateAudit();
    }

    #endregion
}