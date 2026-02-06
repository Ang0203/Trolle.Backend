using Trolle.Domain.Common;

namespace Trolle.Domain.Entities;

/// <summary>
/// Represents a project board containing columns and cards.
/// </summary>
public class Board : BaseEntity
{
    #region Fields

    private readonly List<Column> _columns = new();
    private readonly List<Label> _labels = new();

    #endregion

    #region Properties

    /// <summary>
    /// Gets whether the board is favorited.
    /// </summary>
    public bool IsFavorite { get; private set; }

    /// <summary>
    /// The title of the board.
    /// </summary>
    public Title Title { get; private set; }

    /// <summary>
    /// The title color of the board.
    /// </summary>
    public CssColor TitleColor { get; private set; }

    /// <summary>
    /// The background image URL or color.
    /// </summary>
    public string? BackgroundImage { get; private set; }

    /// <summary>
    /// The background color of the board.
    /// </summary>
    public CssColor BackgroundColor { get; private set; }

    /// <summary>
    /// Gets the list of columns in this board.
    /// </summary>
    public IReadOnlyCollection<Column> Columns => _columns.AsReadOnly();

    /// <summary>
    /// Gets the list of labels available for this board.
    /// </summary>
    public IReadOnlyCollection<Label> Labels => _labels.AsReadOnly();

    #endregion

    #region Constructors

    private Board()
    {
        Title = null!;
        TitleColor = null!;
        BackgroundColor = null!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Board"/> class.
    /// </summary>
    public Board(string title, string? backgroundImage = null)
    {
        Title = Title.Create(title);
        BackgroundImage = backgroundImage;
        TitleColor = CssColor.DefaultTitleColor;
        BackgroundColor = CssColor.DefaultBoardBackground;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Toggles the favorite status.
    /// </summary>
    public void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
        UpdateAudit();
    }

    /// <summary>
    /// Updates the board title.
    /// </summary>
    /// <param name="title">The new title.</param>
    public void UpdateTitle(string title)
    {
        Title = Title.Create(title);
        UpdateAudit();
    }

    /// <summary>
    /// Updates the board title color.
    /// </summary>
    /// <param name="color">The new CSS color string.</param>
    public void UpdateTitleColor(string color)
    {
        TitleColor = CssColor.Create(color);
        UpdateAudit();
    }

    /// <summary>
    /// Updates the board background color.
    /// </summary>
    /// <param name="color">The new CSS color string.</param>
    public void UpdateBackgroundColor(string color)
    {
        BackgroundColor = CssColor.Create(color);
        UpdateAudit();
    }

    /// <summary>
    /// Adds a new column to the board.
    /// </summary>
    /// <param name="title">The title of the column.</param>
    /// <param name="order">The display order of the column.</param>
    /// <param name="headerColor">The background color of the column header.</param>
    public void AddColumn(string title, int order, string headerColor = "transparent")
    {
        var column = new Column(title, order, this.Id, headerColor);
        _columns.Add(column);
        UpdateAudit();
    }

    /// <summary>
    /// Removes a column from the board.
    /// </summary>
    /// <param name="columnId">The unique identifier of the column to remove.</param>
    public void RemoveColumn(Guid columnId)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column != null)
        {
            _columns.Remove(column);
            UpdateAudit();
        }
    }

    /// <summary>
    /// Adds a label to the board.
    /// </summary>
    /// <param name="name">The name of the label.</param>
    /// <param name="color">The background color of the label.</param>
    /// <param name="textColor">The text color of the label.</param>
    public void AddLabel(string name, string color, string textColor)
    {
        var label = new Label(name, color, textColor, this.Id);
        _labels.Add(label);
        UpdateAudit();
    }

    /// <summary>
    /// Removes a label from the board.
    /// </summary>
    /// <param name="labelId">The unique identifier of the label to remove.</param>
    public void RemoveLabel(Guid labelId)
    {
        var label = _labels.FirstOrDefault(l => l.Id == labelId);
        if (label != null)
        {
            _labels.Remove(label);
            UpdateAudit();
        }
    }

    #endregion
}