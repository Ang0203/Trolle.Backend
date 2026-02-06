namespace Trolle.Infrastructure.Options;

/// <summary>
/// OpenTelemetry options.
/// </summary>
public class OpenTelemetryOptions
{
    /// <summary>
    /// Gets or sets the endpoint URL used to connect to the service.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the service.
    /// </summary>
    public string ServiceName { get; set; } = "Trolle.API";
}