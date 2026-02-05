namespace Trolle.Infrastructure.Options;

public class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxRetries { get; set; } = 10;
    public int DelaySeconds { get; set; } = 3;
}