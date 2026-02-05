using Serilog.Events;
using Serilog.Core;
using System.Diagnostics;

namespace Trolle.API.Observability;

/// <summary>
/// Enriches log events with current Activity TraceId and SpanId.
/// </summary>
public sealed class ActivityEnricher : ILogEventEnricher
{
    /// <summary>
    /// Enriches the log event.
    /// </summary>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
    {
        var activity = Activity.Current;
        if (activity == null) return;

        logEvent.AddPropertyIfAbsent(factory.CreateProperty(
            "TraceId", activity.TraceId.ToString()));

        logEvent.AddPropertyIfAbsent(factory.CreateProperty(
            "SpanId", activity.SpanId.ToString()));
    }
}