namespace Trolle.Application.DTOs;

public class LabelDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string TextColor { get; set; }
    public required string Color { get; set; }
    public Guid BoardId { get; set; }
    public uint RowVersion { get; set; }
}