using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.RateLimiting;
using Trolle.Application.Interfaces;
using Trolle.API.Hubs;
using Trolle.API.Requests;

namespace Trolle.API.Controllers;

/// <summary>
/// Controller for managing labels.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LabelController : ControllerBase
{
    private readonly ILabelService _labelService;
    private readonly IHubContext<BoardHub> _hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LabelController"/> class.
    /// </summary>
    /// <param name="labelService">The label service.</param>
    /// <param name="hubContext">The SignalR hub context.</param>
    public LabelController(ILabelService labelService, IHubContext<BoardHub> hubContext)
    {
        _labelService = labelService;
        _hubContext = hubContext;
    }

    #region Label Actions

    /// <summary>
    /// Creates a label for a board.
    /// </summary>
    /// <param name="request">The label creation request.</param>
    [HttpPost]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> CreateLabel([FromBody] CreateLabelRequest request)
    {
        var id = await _labelService.CreateLabelAsync(request.BoardId, request.Name, request.Color, request.TextColor);
        
        if (request.BoardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return Created("", new { id });
    }

    /// <summary>
    /// Updates a label.
    /// </summary>
    /// <param name="id">The label ID.</param>
    /// <param name="request">The label update request.</param>
    [HttpPut("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UpdateLabel(Guid id, [FromBody] UpdateLabelRequest request)
    {
        await _labelService.UpdateLabelAsync(id, request.Name, request.Color, request.TextColor);
        
        if (request.BoardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return Ok();
    }

    /// <summary>
    /// Deletes a label.
    /// </summary>
    /// <param name="id">The label ID.</param>
    /// <param name="boardId">The board ID for notification.</param>
    [HttpDelete("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> DeleteLabel(Guid id, [FromQuery] Guid boardId)
    {
        await _labelService.DeleteLabelAsync(id);
        
        if (boardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(boardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return NoContent();
    }

    #endregion
}
