namespace Trolle.Infrastructure.Options;

/// <summary>
/// SignalR options.
/// </summary>
public class SignalROptions
{
    /// <summary>
    /// Path to the Board Hub.
    /// </summary>
    public string BoardHubPath { get; set; } = string.Empty;
}