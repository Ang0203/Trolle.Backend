namespace Trolle.Infrastructure.Options;

/// <summary>
/// Database configuration options.
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Gets or sets the connection string used to establish a connection to the data source.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum number of times an operation is retried after a failure.
    /// </summary>
    public int MaxRetries { get; set; } = 10;

    /// <summary>
    /// Gets or sets the delay duration, in seconds, before the operation is executed.
    /// </summary>
    public int DelaySeconds { get; set; } = 3;
}