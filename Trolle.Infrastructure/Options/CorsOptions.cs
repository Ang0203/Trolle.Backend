namespace Trolle.Infrastructure.Options;

/// <summary>
/// CORS options.
/// </summary>
public class CorsOptions
{
    /// <summary>
    /// Gets or sets the list of allowed origins for cross-origin requests.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];
}