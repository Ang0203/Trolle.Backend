namespace Trolle.Application.DTOs;

public class BoardDto
{
    public Guid Id { get; set; }
    public bool IsFavorite { get; set; }
    public required string Title { get; set; }
    public required string TitleColor { get; set; }
    public string? BackgroundImage { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string BackgroundColor { get; set; }
    public List<ColumnDto> Columns { get; set; } = new();
    public List<LabelDto> Labels { get; set; } = new();
    public uint RowVersion { get; set; }
}