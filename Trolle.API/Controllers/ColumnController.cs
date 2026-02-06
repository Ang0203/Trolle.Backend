using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.RateLimiting;
using Trolle.Application.Interfaces;
using Trolle.API.Hubs;
using Trolle.API.Requests;

namespace Trolle.API.Controllers;

/// <summary>
/// Controller for managing columns within boards.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ColumnController : ControllerBase
{
    private readonly IColumnService _columnService;
    private readonly IHubContext<BoardHub> _hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnController"/> class.
    /// </summary>
    /// <param name="columnService">The column service.</param>
    /// <param name="hubContext">The SignalR hub context.</param>
    public ColumnController(IColumnService columnService, IHubContext<BoardHub> hubContext)
    {
        _columnService = columnService;
        _hubContext = hubContext;
    }

    #region Column Actions

    /// <summary>
    /// Creates a column in a board.
    /// </summary>
    /// <param name="boardId">The board ID.</param>
    /// <param name="request">The column creation request.</param>
    [HttpPost("~/api/Board/{boardId}/columns")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> CreateColumn(Guid boardId, [FromBody] CreateColumnRequest request)
    {
        await _columnService.CreateColumnAsync(boardId, request.Title, request.HeaderColor);
        await _hubContext.Clients.Group(boardId.ToString()).SendAsync("BoardUpdated");
        return Ok();
    }

    /// <summary>
    /// Updates a column.
    /// </summary>
    /// <param name="id">The column ID.</param>
    /// <param name="request">The column update request.</param>
    [HttpPut("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UpdateColumn(Guid id, [FromBody] UpdateColumnRequest request)
    {
        await _columnService.UpdateColumnAsync(request.BoardId, id, request.Title, request.TitleColor, request.HeaderColor);
        
        if (request.BoardId != Guid.Empty)
        {
            await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return Ok();
    }

    /// <summary>
    /// Deletes a column.
    /// </summary>
    /// <param name="id">The column ID.</param>
    /// <param name="boardId">The board ID.</param>
    [HttpDelete("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> DeleteColumn(Guid id, [FromQuery] Guid boardId)
    {
        await _columnService.DeleteColumnAsync(boardId, id);
        
        if (boardId != Guid.Empty)
        {
            await _hubContext.Clients.Group(boardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return NoContent();
    }

    #endregion

    #region Movement Actions

    /// <summary>
    /// Moves a column to a new position.
    /// </summary>
    /// <param name="request">The column move request.</param>
    [HttpPut("move")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> MoveColumn([FromBody] MoveColumnRequest request)
    {
        await _columnService.MoveColumnAsync(request.BoardId, request.ColumnId, request.NewOrder);
        
        if (request.BoardId != Guid.Empty)
        {
            await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        }

        return Ok();
    }

    /// <summary>
    /// Bulk moves columns.
    /// </summary>
    /// <param name="request">The bulk column move request.</param>
    [HttpPut("bulk-move")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> BulkMoveColumns([FromBody] BulkMoveColumnsRequest request)
    {
        await _columnService.BulkMoveColumnsAsync(request.BoardId, request.ColumnOrders);
        if (request.BoardId != Guid.Empty)
            await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        return Ok();
    }

    #endregion
}
