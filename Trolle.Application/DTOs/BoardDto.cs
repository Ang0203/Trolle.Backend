using System;
using System.Collections.Generic;

namespace Trolle.Application.DTOs;

public class BoardDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? BackgroundImage { get; set; }
    public required string TitleColor { get; set; }
    public required string BackgroundColor { get; set; }
    public bool IsFavorite { get; set; }
    public List<ColumnDto> Columns { get; set; } = new();
    public List<LabelDto> Labels { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class LabelDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Color { get; set; }
    public required string TextColor { get; set; }
    public Guid BoardId { get; set; }
}

public class ColumnDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string TitleColor { get; set; }
    public required string HeaderColor { get; set; }
    public int Order { get; set; }
    public List<CardDto> Cards { get; set; } = new();
}

public class CardDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int Order { get; set; }
    public Guid ColumnId { get; set; }
    public bool IsArchived { get; set; }
    public List<LabelDto> Labels { get; set; } = new();
}
