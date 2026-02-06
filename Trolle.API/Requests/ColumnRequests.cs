namespace Trolle.API.Requests;

/// <summary>
/// Request to create a column
/// </summary>
public class CreateColumnRequest 
{ 
    /// <summary>
    /// The title of the column
    /// </summary>
    public required string Title { get; set; } 
    
    /// <summary>
    /// The header color of the column
    /// </summary>
    public required string HeaderColor { get; set; } 
}

/// <summary>
/// Request to update a column
/// </summary>
public class UpdateColumnRequest 
{ 
    /// <summary>
    /// The board ID
    /// </summary>
    public Guid BoardId { get; set; } 
    
    /// <summary>
    /// The new title
    /// </summary>
    public required string Title { get; set; } 
    
    /// <summary>
    /// The new title color
    /// </summary>
    public required string TitleColor { get; set; } 
    
    /// <summary>
    /// The new header color
    /// </summary>
    public required string HeaderColor { get; set; } 
}

/// <summary>
/// Request to move a column
/// </summary>
public class MoveColumnRequest 
{ 
    /// <summary>
    /// The board ID
    /// </summary>
    public Guid BoardId { get; set; } 
    
    /// <summary>
    /// The column ID
    /// </summary>
    public Guid ColumnId { get; set; } 
    
    /// <summary>
    /// The new order
    /// </summary>
    public int NewOrder { get; set; } 
}

/// <summary>
/// Request to bulk move columns
/// </summary>
public class BulkMoveColumnsRequest 
{ 
    /// <summary>
    /// The board ID
    /// </summary>
    public Guid BoardId { get; set; } 
    
    /// <summary>
    /// Dictionary of column IDs to their new orders
    /// </summary>
    public required Dictionary<Guid, int> ColumnOrders { get; set; } 
}
