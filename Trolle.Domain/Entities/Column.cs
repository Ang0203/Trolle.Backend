using System.Collections.Generic;
using Trolle.Domain.Common;

namespace Trolle.Domain.Entities;

public class Column : BaseEntity
{
    public string Title { get; private set; }
    public int Order { get; private set; }
    public Guid BoardId { get; private set; }
    public string TitleColor { get; private set; }
    public string HeaderColor { get; private set; }
    
    // Navigation
    private readonly List<Card> _cards = new();
    public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();

    private Column() 
    { 
        Title = null!;
        TitleColor = null!;
        HeaderColor = null!;
    }

    public Column(string title, int order, Guid boardId, string headerColor = "transparent")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        Title = title;
        Order = order;
        BoardId = boardId;
        TitleColor = "#1e293b"; // Default slate-800
        HeaderColor = headerColor;
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

    public void UpdateHeaderColor(string color)
    {
        HeaderColor = color;
        UpdateAudit();
    }

    public void SetOrder(int order)
    {
        Order = order;
        UpdateAudit();
    }
}
