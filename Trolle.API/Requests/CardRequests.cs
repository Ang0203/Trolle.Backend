namespace Trolle.API.Requests;

/// <summary>
/// Request to create a card
/// </summary>
public class CreateCardRequest 
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
    /// The title of the card
    /// </summary>
    public required string Title { get; set; } 
    
    /// <summary>
    /// The description of the card
    /// </summary>
    public string? Description { get; set; } 
    
    /// <summary>
    /// The list of label IDs
    /// </summary>
    public List<Guid> LabelIds { get; set; } = new(); 
}

/// <summary>
/// Request to update a card
/// </summary>
public class UpdateCardRequest 
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
    /// The new description
    /// </summary>
    public string? Description { get; set; } 
    
    /// <summary>
    /// The new list of label IDs
    /// </summary>
    public List<Guid> LabelIds { get; set; } = new(); 
}

/// <summary>
/// Request to move a card
/// </summary>
public class MoveCardRequest 
{ 
    /// <summary>
    /// The board ID
    /// </summary>
    public Guid BoardId { get; set; } 
    
    /// <summary>
    /// The card ID
    /// </summary>
    public Guid CardId { get; set; } 
    
    /// <summary>
    /// The target column ID
    /// </summary>
    public Guid TargetColumnId { get; set; } 
    
    /// <summary>
    /// The new order
    /// </summary>
    public int NewOrder { get; set; } 
}

/// <summary>
/// Request to bulk move cards
/// </summary>
public class BulkMoveCardsRequest 
{ 
    /// <summary>
    /// The board ID
    /// </summary>
    public Guid BoardId { get; set; } 
    
    /// <summary>
    /// Dictionary of card IDs to their new orders
    /// </summary>
    public required Dictionary<Guid, int> CardOrders { get; set; } 
}
