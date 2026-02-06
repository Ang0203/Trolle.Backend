using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.RateLimiting;
using Trolle.Application.Interfaces;
using Trolle.API.Hubs;
using Trolle.API.Requests;

namespace Trolle.API.Controllers;

/// <summary>
/// Controller for managing cards within columns.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CardController : ControllerBase
{
    private readonly ICardService _cardService;
    private readonly IHubContext<BoardHub> _hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CardController"/> class.
    /// </summary>
    /// <param name="cardService">The card service.</param>
    /// <param name="hubContext">The SignalR hub context.</param>
    public CardController(ICardService cardService, IHubContext<BoardHub> hubContext)
    {
        _cardService = cardService;
        _hubContext = hubContext;
    }

    #region Card Actions

    /// <summary>
    /// Creates a card in a column.
    /// </summary>
    /// <param name="request">The card creation request.</param>
    [HttpPost]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> CreateCard([FromBody] CreateCardRequest request)
    {
        await _cardService.CreateCardAsync(request.ColumnId, request.Title, request.Description, request.LabelIds);
        
        if (request.BoardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return Ok();
    }

    /// <summary>
    /// Updates a card.
    /// </summary>
    /// <param name="id">The card ID.</param>
    /// <param name="request">The card update request.</param>
    [HttpPut("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UpdateCard(Guid id, [FromBody] UpdateCardRequest request)
    {
        await _cardService.UpdateCardAsync(id, request.Title, request.Description, request.LabelIds);
        
        if (request.BoardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return Ok();
    }

    /// <summary>
    /// Deletes a card.
    /// </summary>
    /// <param name="id">The card ID.</param>
    /// <param name="boardId">The board ID for notification.</param>
    [HttpDelete("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> DeleteCard(Guid id, [FromQuery] Guid boardId)
    {
        await _cardService.DeleteCardAsync(id);
        
        if (boardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(boardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return NoContent();
    }

    #endregion

    #region Archiving

    /// <summary>
    /// Archives a card.
    /// </summary>
    /// <param name="id">The card ID.</param>
    /// <param name="boardId">The board ID.</param>
    [HttpPost("{id}/archive")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> ArchiveCard(Guid id, [FromQuery] Guid boardId)
    {
        await _cardService.ArchiveCardAsync(id);
        
        if (boardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(boardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return Ok();
    }

    /// <summary>
    /// Unarchives a card.
    /// </summary>
    /// <param name="id">The card ID.</param>
    /// <param name="boardId">The board ID.</param>
    [HttpPost("{id}/unarchive")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UnarchiveCard(Guid id, [FromQuery] Guid boardId)
    {
        await _cardService.UnarchiveCardAsync(id);
        
        if (boardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(boardId.ToString()).SendAsync("BoardUpdated");
        }
        
        return Ok();
    }

    #endregion

    #region Movement

    /// <summary>
    /// Moves a card to a new column or position.
    /// </summary>
    /// <param name="request">The card move request.</param>
    [HttpPut("move")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> MoveCard([FromBody] MoveCardRequest request)
    {
        await _cardService.MoveCardAsync(request.CardId, request.TargetColumnId, request.NewOrder);
        
        if (request.BoardId != Guid.Empty)
        {
             await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        }

        return Ok();
    }

    /// <summary>
    /// Bulk moves cards.
    /// </summary>
    /// <param name="request">The bulk card move request.</param>
    [HttpPut("bulk-move")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> BulkMoveCards([FromBody] BulkMoveCardsRequest request)
    {
        await _cardService.BulkMoveCardsAsync(request.BoardId, request.CardOrders);
        if (request.BoardId != Guid.Empty)
            await _hubContext.Clients.Group(request.BoardId.ToString()).SendAsync("BoardUpdated");
        return Ok();
    }

    #endregion
}
