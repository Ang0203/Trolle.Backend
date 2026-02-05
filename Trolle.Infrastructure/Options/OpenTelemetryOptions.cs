namespace Trolle.Infrastructure.Options;

public class OpenTelemetryOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string ServiceName { get; set; } = "Trolle.API";
}