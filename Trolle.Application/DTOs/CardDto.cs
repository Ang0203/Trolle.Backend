namespace Trolle.Application.DTOs;

public class CardDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int Order { get; set; }
    public bool IsArchived { get; set; }
    public Guid ColumnId { get; set; }
    public List<LabelDto> Labels { get; set; } = new();
    public uint RowVersion { get; set; }
}