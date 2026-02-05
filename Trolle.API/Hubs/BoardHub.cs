using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Trolle.API.Hubs;

/// <summary>
/// Hub for real-time board updates
/// </summary>
public class BoardHub : Hub
{
    /// <summary>
    /// Joins a board group to receive updates for that board
    /// </summary>
    public async Task JoinBoard(string boardId)
    {
        if (string.IsNullOrWhiteSpace(boardId))
            throw new HubException("boardId is required");

        await Groups.AddToGroupAsync(Context.ConnectionId, boardId);
    }

    /// <summary>
    /// Leaves a board group
    /// </summary>
    public async Task LeaveBoard(string boardId)
    {
        if (string.IsNullOrWhiteSpace(boardId))
            throw new HubException("boardId is required");
            
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, boardId);
    }

    /// <summary>
    /// Joins the dashboard group to receive updates for the board list
    /// </summary>
    public async Task JoinDashboard()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Dashboard");
    }

    /// <summary>
    /// Leaves the dashboard group
    /// </summary>
    public async Task LeaveDashboard()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Dashboard");
    }
}
