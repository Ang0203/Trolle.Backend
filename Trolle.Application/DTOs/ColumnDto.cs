namespace Trolle.Application.DTOs;

public class ColumnDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string TitleColor { get; set; }
    public required string HeaderColor { get; set; }
    public int Order { get; set; }
    public List<CardDto> Cards { get; set; } = new();
    public uint RowVersion { get; set; }
}