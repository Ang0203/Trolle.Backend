namespace Trolle.API.Requests;

/// <summary>
/// Request to create a label
/// </summary>
public class CreateLabelRequest 
{ 
    /// <summary>
    /// The board ID
    /// </summary>
    public Guid BoardId { get; set; } 
    
    /// <summary>
    /// The label name
    /// </summary>
    public required string Name { get; set; } 
    
    /// <summary>
    /// The label color
    /// </summary>
    public required string Color { get; set; } 
    
    /// <summary>
    /// The text color
    /// </summary>
    public required string TextColor { get; set; } 
}

/// <summary>
/// Request to update a label
/// </summary>
public class UpdateLabelRequest 
{ 
    /// <summary>
    /// The board ID
    /// </summary>
    public Guid BoardId { get; set; } 
    
    /// <summary>
    /// The label name
    /// </summary>
    public required string Name { get; set; } 
    
    /// <summary>
    /// The label color
    /// </summary>
    public required string Color { get; set; } 
    
    /// <summary>
    /// The text color
    /// </summary>
    public required string TextColor { get; set; } 
}
