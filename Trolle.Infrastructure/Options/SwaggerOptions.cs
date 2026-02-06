namespace Trolle.Infrastructure.Options;

/// <summary>
/// Swagger options.
/// </summary>
public class SwaggerOptions
{
    /// <summary>
    /// Gets or sets the title associated with the API.
    /// </summary>
    public string Title { get; set; } = "API";

    /// <summary>
    /// Gets or sets the description for the API documentation.
    /// </summary>
    public string Description { get; set; } = "API Documentation";

    /// <summary>
    /// Gets or sets the API version identifier.
    /// </summary>
    public string Version { get; set; } = "v1";
}