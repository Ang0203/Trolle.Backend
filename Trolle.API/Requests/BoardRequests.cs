namespace Trolle.API.Requests;

/// <summary>
/// Request to create a new board
/// </summary>
public class CreateBoardRequest 
{ 
    /// <summary>
    /// The title of the board
    /// </summary>
    public required string Title { get; set; } 
    
    /// <summary>
    /// The background image URL or color
    /// </summary>
    public required string BackgroundImage { get; set; } 
}

/// <summary>
/// Request to update board title
/// </summary>
public class UpdateBoardTitleRequest 
{ 
    /// <summary>
    /// The new title
    /// </summary>
    public required string Title { get; set; } 
}

/// <summary>
/// Request to update board title color
/// </summary>
public class UpdateBoardColorRequest 
{ 
    /// <summary>
    /// The new color
    /// </summary>
    public required string Color { get; set; } 
}

/// <summary>
/// Request to update board background color
/// </summary>
public class UpdateBoardBackgroundColorRequest 
{ 
    /// <summary>
    /// The new background color
    /// </summary>
    public required string Color { get; set; } 
}
