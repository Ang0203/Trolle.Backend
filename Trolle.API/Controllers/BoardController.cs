using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Threading.Tasks;
using Trolle.Application.DTOs;
using Trolle.Application.Interfaces;
using Trolle.API.Hubs;

namespace Trolle.API.Controllers;

/// <summary>
/// Controller for managing boards, columns, cards, and labels
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BoardController : ControllerBase
{
    private readonly IBoardService _boardService;
    private readonly IHubContext<BoardHub> _hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoardController"/> class.
    /// </summary>
    /// <param name="boardService">The board service</param>
    /// <param name="hubContext">The SignalR hub context</param>
    public BoardController(IBoardService boardService, IHubContext<BoardHub> hubContext)
    {
        _boardService = boardService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Gets all boards
    /// </summary>
    [HttpGet]
    [EnableRateLimiting("read")]
    public async Task<IActionResult> GetBoards()
    {
        var boards = await _boardService.GetBoardsAsync();
        return Ok(boards);
    }

    /// <summary>
    /// Gets a specific board by ID
    /// </summary>
    [HttpGet("{id}")]
    [EnableRateLimiting("read")]
    public async Task<IActionResult> GetBoard(Guid id)
    {
        var board = await _boardService.GetBoardAsync(id);
        if (board == null) return NotFound();
        return Ok(board);
    }

    /// <summary>
    /// Creates a new board
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest request)
    {
        var id = await _boardService.CreateBoardAsync(request.Title, request.BackgroundImage);
        // Notify? Maybe not needed for list view yet.
        return CreatedAtAction(nameof(GetBoard), new { id }, new { id });
    }

    /// <summary>
    /// Toggles the favorite status of a board
    /// </summary>
    [HttpPost("{id}/favorite")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> ToggleFavorite(Guid id)
    {
        await _boardService.ToggleFavoriteAsync(id);
        return Ok();
    }

    /// <summary>
    /// Updates the board title
    /// </summary>
    [HttpPut("{id}/title")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UpdateTitle(Guid id, [FromBody] UpdateBoardTitleRequest request)
    {
        await _boardService.UpdateTitleAsync(id, request.Title);
        
        // Notify the specific board viewers
        await _hubContext.Clients.Group(id.ToString()).SendAsync("BoardUpdated");
        
        // Notify the dashboard viewers (for title change in list)
        await _hubContext.Clients.Group("Dashboard").SendAsync("DashboardUpdated");
        
        return Ok();
    }

    /// <summary>
    /// Creates a column in a board
    /// </summary>
    [HttpPost("{id}/columns")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> CreateColumn(Guid id, [FromBody] CreateColumnRequest request)
    {
        await _boardService.CreateColumnAsync(id, request.Title, request.HeaderColor);
        
        // Notify board group
        await _hubContext.Clients.Group(id.ToString()).SendAsync("BoardUpdated");
        
        return Ok();
    }

    /// <summary>
    /// Creates a card in a column
    /// </summary>
    [HttpPost("cards")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> CreateCard([FromBody] CreateCardRequest request)
    {
        await _boardService.CreateCardAsync(request.ColumnId, request.Title, request.Description, request.LabelIds);
        
        if (request.BoardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return Ok();
    }

    /// <summary>
    /// Updates a card
    /// </summary>
    [HttpPut("cards/{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UpdateCard(Guid id, [FromBody] UpdateCardRequest request)
    {
        await _boardService.UpdateCardAsync(id, request.Title, request.Description, request.LabelIds);
        
        if (request.BoardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return Ok();
    }

    /// <summary>
    /// Deletes a card
    /// </summary>
    [HttpDelete("cards/{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> DeleteCard(Guid id, [FromQuery] Guid boardId)
    {
        await _boardService.DeleteCardAsync(id);
        
        if (boardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(boardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return NoContent();
    }

    /// <summary>
    /// Archives a card
    /// </summary>
    [HttpPost("cards/{id}/archive")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> ArchiveCard(Guid id, [FromQuery] Guid boardId)
    {
        await _boardService.ArchiveCardAsync(id);
        
        if (boardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(boardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return Ok();
    }

    /// <summary>
    /// Unarchives a card
    /// </summary>
    [HttpPost("cards/{id}/unarchive")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UnarchiveCard(Guid id, [FromQuery] Guid boardId)
    {
        await _boardService.UnarchiveCardAsync(id);
        
        if (boardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(boardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return Ok();
    }

    /// <summary>
    /// Moves a card
    /// </summary>
    [HttpPut("cards/move")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> MoveCard([FromBody] MoveCardRequest request)
    {
        await _boardService.MoveCardAsync(request.CardId, request.TargetColumnId, request.NewOrder);
        
        if (request.BoardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        }

        return Ok();
    }

    /// <summary>
    /// Creates a label for a board
    /// </summary>
    [HttpPost("labels")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> CreateLabel([FromBody] CreateLabelRequest request)
    {
        var id = await _boardService.CreateLabelAsync(request.BoardId, request.Name, request.Color, request.TextColor);
        
        if (request.BoardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return Created("", new { id });
    }

    /// <summary>
    /// Updates a label
    /// </summary>
    [HttpPut("labels/{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UpdateLabel(Guid id, [FromBody] UpdateLabelRequest request)
    {
        await _boardService.UpdateLabelAsync(id, request.Name, request.Color, request.TextColor);
        
        if (request.BoardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return Ok();
    }

    /// <summary>
    /// Deletes a label
    /// </summary>
    [HttpDelete("labels/{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> DeleteLabel(Guid id, [FromQuery] Guid boardId)
    {
        await _boardService.DeleteLabelAsync(id);
        
        if (boardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(boardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return NoContent();
    }

    /// <summary>
    /// Updates the board title color
    /// </summary>
    [HttpPut("{id}/color")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UpdateBoardTitleColor(Guid id, [FromBody] UpdateBoardColorRequest request)
    {
        await _boardService.UpdateBoardTitleColorAsync(id, request.Color);
        
        // Notify board group and dashboard
        await _hubContext.Clients.Group(id.ToString()).SendAsync("BoardUpdated");
        await _hubContext.Clients.Group("Dashboard").SendAsync("DashboardUpdated");
        
        return Ok();
    }

    /// <summary>
    /// Updates the board background color
    /// </summary>
    [HttpPut("{id}/background-color")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UpdateBoardBackgroundColor(Guid id, [FromBody] UpdateBoardBackgroundColorRequest request)
    {
        await _boardService.UpdateBoardBackgroundColorAsync(id, request.Color);
        
        // Notify board group and dashboard
        await _hubContext.Clients.Group(id.ToString()).SendAsync("BoardUpdated");
        await _hubContext.Clients.Group("Dashboard").SendAsync("DashboardUpdated");
        
        return Ok();
    }

    /// <summary>
    /// Updates a column
    /// </summary>
    [HttpPut("columns/{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UpdateColumn(Guid id, [FromBody] UpdateColumnRequest request)
    {
        await _boardService.UpdateColumnAsync(request.BoardId, id, request.Title, request.TitleColor, request.HeaderColor);
        
        // We really need boardId here to notify. 
        // Request should ideally pass it.
        if (request.BoardId != Guid.Empty)
        {
            await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return Ok();
    }

    /// <summary>
    /// Moves a column
    /// </summary>
    [HttpPut("columns/move")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> MoveColumn([FromBody] MoveColumnRequest request)
    {
        await _boardService.MoveColumnAsync(request.BoardId, request.ColumnId, request.NewOrder);
        
        if (request.BoardId != Guid.Empty)
        {
            await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        }

        return Ok();
    }

    /// <summary>
    /// Bulk moves columns
    /// </summary>
    [HttpPut("columns/bulk-move")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> BulkMoveColumns([FromBody] BulkMoveColumnsRequest request)
    {
        await _boardService.BulkMoveColumnsAsync(request.BoardId, request.ColumnOrders);
        if (request.BoardId != Guid.Empty)
            await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        return Ok();
    }

    /// <summary>
    /// Bulk moves cards
    /// </summary>
    [HttpPut("cards/bulk-move")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> BulkMoveCards([FromBody] BulkMoveCardsRequest request)
    {
        await _boardService.BulkMoveCardsAsync(request.BoardId, request.CardOrders);
        if (request.BoardId != Guid.Empty)
            await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        return Ok();
    }

    /// <summary>
    /// Deletes a column
    /// </summary>
    [HttpDelete("columns/{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> DeleteColumn(Guid id, [FromQuery] Guid boardId)
    {
        await _boardService.DeleteColumnAsync(boardId, id);
        
        if (boardId != Guid.Empty)
        {
            await _hubContext.Clients.Group(boardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return NoContent();
    }

    /// <summary>
    /// Deletes a board
    /// </summary>
    [HttpDelete("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> DeleteBoard(Guid id)
    {
        await _boardService.DeleteBoardAsync(id);
        
        // Notify dashboard to refresh list
        await _hubContext.Clients.Group("Dashboard").SendAsync("DashboardUpdated");
        
        return NoContent();
    }
}

/// <summary>
/// Request to update board title color
/// </summary>
public class UpdateBoardColorRequest { 
    /// <summary>
    /// The new color
    /// </summary>
    public required string Color { get; set; } 
}

/// <summary>
/// Request to update board background color
/// </summary>
public class UpdateBoardBackgroundColorRequest { 
    /// <summary>
    /// The new background color
    /// </summary>
    public required string Color { get; set; } 
}

/// <summary>
/// Request to update a column
/// </summary>
public class UpdateColumnRequest { 
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
/// Request to create a new board
/// </summary>
public class CreateBoardRequest { 
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
public class UpdateBoardTitleRequest { 
    /// <summary>
    /// The new title
    /// </summary>
    public required string Title { get; set; } 
}

/// <summary>
/// Request to create a column
/// </summary>
public class CreateColumnRequest { 
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
/// Request to create a card
/// </summary>
public class CreateCardRequest { 
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
public class UpdateCardRequest { 
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
public class MoveCardRequest { 
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
/// Request to move a column
/// </summary>
public class MoveColumnRequest { 
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
public class BulkMoveColumnsRequest { 
    /// <summary>
    /// The board ID
    /// </summary>
    public Guid BoardId { get; set; } 
    /// <summary>
    /// Dictionary of column IDs to their new orders
    /// </summary>
    public required Dictionary<Guid, int> ColumnOrders { get; set; } 
}

/// <summary>
/// Request to bulk move cards
/// </summary>
public class BulkMoveCardsRequest { 
    /// <summary>
    /// The board ID
    /// </summary>
    public Guid BoardId { get; set; } 
    /// <summary>
    /// Dictionary of card IDs to their new orders
    /// </summary>
    public required Dictionary<Guid, int> CardOrders { get; set; } 
}

/// <summary>
/// Request to create a label
/// </summary>
public class CreateLabelRequest { 
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
public class UpdateLabelRequest { 
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
