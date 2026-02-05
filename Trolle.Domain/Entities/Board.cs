using System;
using System.Collections.Generic;
using Trolle.Domain.Common;

namespace Trolle.Domain.Entities;

public class Board : BaseEntity
{
    public string Title { get; private set; }
    
    // Navigation
    private readonly List<Column> _columns = new();
    public IReadOnlyCollection<Column> Columns => _columns.AsReadOnly();

    private readonly List<Label> _labels = new();
    public IReadOnlyCollection<Label> Labels => _labels.AsReadOnly();

    private Board() 
    { 
        Title = null!;
        TitleColor = null!;
        BackgroundColor = null!;
    }

    public string? BackgroundImage { get; private set; }
    public string TitleColor { get; private set; }
    public string BackgroundColor { get; private set; }

    public Board(string title, string? backgroundImage = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));
        Title = title;
        BackgroundImage = backgroundImage;
        TitleColor = "#1e293b"; // Default slate-800
        BackgroundColor = "#1e293b"; // Default slate-800
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));
        Title = title;
        UpdateAudit();
    }

    public void UpdateTitleColor(string color)
    {
        TitleColor = color;
        UpdateAudit();
    }

    public void UpdateBackgroundColor(string color)
    {
        BackgroundColor = color;
        UpdateAudit();
    }

    public void AddColumn(string title, int order, string headerColor = "transparent")
    {
        var column = new Column(title, order, this.Id, headerColor);
        _columns.Add(column);
        UpdateAudit();
    }

    public bool IsFavorite { get; private set; }

    public void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
        UpdateAudit();
    }

    public void RemoveColumn(Guid columnId)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column != null)
        {
            _columns.Remove(column);
            UpdateAudit();
        }
    }

    public void AddLabel(string name, string color, string textColor)
    {
        var label = new Label(name, color, textColor, this.Id);
        _labels.Add(label);
        UpdateAudit();
    }

    public void RemoveLabel(Guid labelId)
    {
        var label = _labels.FirstOrDefault(l => l.Id == labelId);
        if (label != null)
        {
            _labels.Remove(label);
            UpdateAudit();
        }
    }
}
