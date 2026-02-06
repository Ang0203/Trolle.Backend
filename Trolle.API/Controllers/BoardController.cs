using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.RateLimiting;
using Trolle.Application.Interfaces;
using Trolle.API.Hubs;
using Trolle.API.Requests;

namespace Trolle.API.Controllers;

/// <summary>
/// Controller for managing boards.
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
    /// <param name="boardService">The board service.</param>
    /// <param name="hubContext">The SignalR hub context.</param>
    public BoardController(IBoardService boardService, IHubContext<BoardHub> hubContext)
    {
        _boardService = boardService;
        _hubContext = hubContext;
    }

    #region Board Queries

    /// <summary>
    /// Gets all boards.
    /// </summary>
    [HttpGet]
    [EnableRateLimiting("read")]
    public async Task<IActionResult> GetBoards()
    {
        var boards = await _boardService.GetBoardsAsync();
        return Ok(boards);
    }

    /// <summary>
    /// Gets a specific board by ID.
    /// </summary>
    /// <param name="id">The board ID.</param>
    [HttpGet("{id}")]
    [EnableRateLimiting("read")]
    public async Task<IActionResult> GetBoard(Guid id)
    {
        var board = await _boardService.GetBoardAsync(id);
        if (board == null) return NotFound();
        return Ok(board);
    }

    #endregion

    #region Board Actions

    /// <summary>
    /// Creates a new board.
    /// </summary>
    /// <param name="request">The board creation request.</param>
    [HttpPost]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest request)
    {
        var id = await _boardService.CreateBoardAsync(request.Title, request.BackgroundImage);
        return CreatedAtAction(nameof(GetBoard), new { id }, new { id });
    }

    /// <summary>
    /// Toggles the favorite status of a board.
    /// </summary>
    /// <param name="id">The board ID.</param>
    [HttpPost("{id}/favorite")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> ToggleFavorite(Guid id)
    {
        await _boardService.ToggleFavoriteAsync(id);
        return Ok();
    }

    /// <summary>
    /// Updates the board title.
    /// </summary>
    /// <param name="id">The board ID.</param>
    /// <param name="request">The title update request.</param>
    [HttpPut("{id}/title")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UpdateTitle(Guid id, [FromBody] UpdateBoardTitleRequest request)
    {
        await _boardService.UpdateTitleAsync(id, request.Title);
        
        await _hubContext.Clients.Group(id.ToString()).SendAsync("BoardUpdated");
        await _hubContext.Clients.Group("Dashboard").SendAsync("DashboardUpdated");
        
        return Ok();
    }

    /// <summary>
    /// Updates the board title color.
    /// </summary>
    /// <param name="id">The board ID.</param>
    /// <param name="request">The color update request.</param>
    [HttpPut("{id}/color")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UpdateBoardTitleColor(Guid id, [FromBody] UpdateBoardColorRequest request)
    {
        await _boardService.UpdateBoardTitleColorAsync(id, request.Color);
        
        await _hubContext.Clients.Group(id.ToString()).SendAsync("BoardUpdated");
        await _hubContext.Clients.Group("Dashboard").SendAsync("DashboardUpdated");
        
        return Ok();
    }

    /// <summary>
    /// Updates the board background color.
    /// </summary>
    /// <param name="id">The board ID.</param>
    /// <param name="request">The background color update request.</param>
    [HttpPut("{id}/background-color")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UpdateBoardBackgroundColor(Guid id, [FromBody] UpdateBoardBackgroundColorRequest request)
    {
        await _boardService.UpdateBoardBackgroundColorAsync(id, request.Color);
        
        await _hubContext.Clients.Group(id.ToString()).SendAsync("BoardUpdated");
        await _hubContext.Clients.Group("Dashboard").SendAsync("DashboardUpdated");
        
        return Ok();
    }

    /// <summary>
    /// Deletes a board.
    /// </summary>
    /// <param name="id">The board ID.</param>
    [HttpDelete("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> DeleteBoard(Guid id)
    {
        await _boardService.DeleteBoardAsync(id);
        await _hubContext.Clients.Group("Dashboard").SendAsync("DashboardUpdated");
        return NoContent();
    }

    #endregion
}
