using Trolle.Domain.Common;
using System;
using System.Collections.Generic;

namespace Trolle.Domain.Entities;

public class Label : BaseEntity
{
    public string Name { get; private set; }
    public string Color { get; private set; }
    public string TextColor { get; private set; }
    
    public Guid BoardId { get; private set; }

    // Navigation properties for many-to-many
    private readonly List<Card> _cards = new();
    public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();

    private Label() 
    { 
        Name = null!;
        Color = null!;
        TextColor = null!;
    }

    public Label(string name, string color, string textColor, Guid boardId)
    {
        Name = name;
        Color = color;
        TextColor = textColor;
        BoardId = boardId;
    }

    public void Update(string name, string color, string textColor)
    {
        Name = name;
        Color = color;
        TextColor = textColor;
        UpdateAudit();
    }
}
