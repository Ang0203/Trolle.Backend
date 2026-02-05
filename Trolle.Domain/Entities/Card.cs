using Trolle.Domain.Common;

namespace Trolle.Domain.Entities;

public class Card : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int Order { get; private set; }
    public bool IsArchived { get; private set; }
    
    public Guid ColumnId { get; private set; }
    // Navigation property
    // public Column Column { get; private set; } // Avoid cyclic dependency issues in serialization, but useful in EF

    private Card() 
    { 
        Title = null!;
        Description = null!;
    } // For EF Core

    public Card(string title, string description, int order, Guid columnId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        Title = title;
        Description = description;
        Order = order;
        ColumnId = columnId;
        IsArchived = false;
    }

    public void Archive()
    {
        IsArchived = true;
        UpdateAudit();
    }

    public void Unarchive()
    {
        IsArchived = false;
        UpdateAudit();
    }

    public void Update(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));
        
        Title = title;
        Description = description;
        UpdateAudit();
    }

    public void SetOrder(int order)
    {
        Order = order;
        UpdateAudit();
    }

    public void MoveToColumn(Guid columnId)
    {
        ColumnId = columnId;
        UpdateAudit();
    }

    private readonly List<Label> _labels = new();
    public IReadOnlyCollection<Label> Labels => _labels.AsReadOnly();

    public void SetLabels(IEnumerable<Label> labels)
    {
        _labels.Clear();
        _labels.AddRange(labels);
        UpdateAudit();
    }

    public void AddLabel(Label label)
    {
        if (!_labels.Contains(label))
        {
            _labels.Add(label);
            UpdateAudit();
        }
    }

    public void RemoveLabel(Label label)
    {
        if (_labels.Contains(label))
        {
            _labels.Remove(label);
            UpdateAudit();
        }
    }
}
