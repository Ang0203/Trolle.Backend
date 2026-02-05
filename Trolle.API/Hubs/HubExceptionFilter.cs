using Microsoft.AspNetCore.SignalR;

/// <summary>
/// Filter to handle exceptions in Hub methods
/// </summary>
public class HubExceptionFilter : IHubFilter
{
    /// <summary>
    /// Intercepts hub method invocations to handle exceptions
    /// </summary>
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            return await next(invocationContext);
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new HubException("Internal server error", ex);
        }
    }
}
