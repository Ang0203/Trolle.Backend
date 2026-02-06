using Trolle.Domain.Common;

namespace Trolle.Domain.Entities;

/// <summary>
/// Represents a card within a column.
/// </summary>
public class Card : BaseEntity
{
    #region Fields

    private readonly List<Label> _labels = new();

    #endregion

    #region Properties

    /// <summary>
    /// The title of the card.
    /// </summary>
    public Title Title { get; private set; }

    /// <summary>
    /// The detailed description of the card.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// The display order of the card within its column.
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// Gets whether the card is archived.
    /// </summary>
    public bool IsArchived { get; private set; }

    /// <summary>
    /// The ID of the column this card belongs to.
    /// </summary>
    public Guid ColumnId { get; private set; }

    /// <summary>
    /// Gets the list of labels assigned to this card.
    /// </summary>
    public IReadOnlyCollection<Label> Labels => _labels.AsReadOnly();

    #endregion

    #region Constructors

    private Card()
    {
        Title = null!;
        Description = null!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Card"/> class.
    /// </summary>
    public Card(string title, string description, int order, Guid columnId)
    {
        Title = Title.Create(title);
        Description = description;
        Order = order;
        ColumnId = columnId;
        IsArchived = false;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Archives the card.
    /// </summary>
    public void Archive()
    {
        IsArchived = true;
        UpdateAudit();
    }

    /// <summary>
    /// Unarchives the card.
    /// </summary>
    public void Unarchive()
    {
        IsArchived = false;
        UpdateAudit();
    }

    /// <summary>
    /// Updates the card title and description.
    /// </summary>
    /// <param name="title">The new title of the card.</param>
    /// <param name="description">The new description of the card.</param>
    public void Update(string title, string description)
    {
        Title = Title.Create(title);
        Description = description;
        UpdateAudit();
    }

    /// <summary>
    /// Sets the card order.
    /// </summary>
    /// <param name="order">The new display order.</param>
    public void SetOrder(int order)
    {
        Order = order;
        UpdateAudit();
    }

    /// <summary>
    /// Moves the card to a different column.
    /// </summary>
    /// <param name="columnId">The unique identifier of the target column.</param>
    public void MoveToColumn(Guid columnId)
    {
        ColumnId = columnId;
        UpdateAudit();
    }

    /// <summary>
    /// Replaces all labels on the card.
    /// </summary>
    /// <param name="labels">The collection of labels to assign.</param>
    public void SetLabels(IEnumerable<Label> labels)
    {
        _labels.Clear();
        _labels.AddRange(labels);
        UpdateAudit();
    }

    /// <summary>
    /// Adds a label to the card.
    /// </summary>
    /// <param name="label">The label to add.</param>
    public void AddLabel(Label label)
    {
        if (!_labels.Any(l => l.Id == label.Id))
        {
            _labels.Add(label);
            UpdateAudit();
        }
    }

    /// <summary>
    /// Removes a label from the card.
    /// </summary>
    /// <param name="label">The label to remove.</param>
    public void RemoveLabel(Label label)
    {
        var existing = _labels.FirstOrDefault(l => l.Id == label.Id);
        if (existing != null)
        {
            _labels.Remove(existing);
            UpdateAudit();
        }
    }

    #endregion
}