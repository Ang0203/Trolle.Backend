using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Trolle.API.Hubs
{
    /// <summary>
    /// Simple per-connection message rate limiter implemented as an IHubFilter.
    /// Limits the number of hub method invocations per connection within a window.
    /// In-memory only; use Redis or a distributed store for multi-instance deployments.
    /// 
    /// Features:
    /// - InvokeMethodAsync enforces per-connection permit limit.
    /// - OnConnectedAsync initializes the connection counter.
    /// - OnDisconnectedAsync removes the connection counter.
    /// - Background cleanup (Timer) removes stale counters that haven't been seen for a configurable duration.
    /// </summary>
    public class HubMessageRateLimitFilter : IHubFilter, IDisposable
    {
        // configure here:
        private readonly int _permitLimit = 60; // e.g. 60 hub method calls per window
        private readonly TimeSpan _window = TimeSpan.FromMinutes(1);

        // cleanup settings
        private readonly TimeSpan _staleThreshold = TimeSpan.FromMinutes(5); // if no activity for this duration, remove counter
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(1);

        private class Counter
        {
            public DateTime WindowStartUtc;
            public int Count;
            public DateTime LastSeenUtc;
        }

        private readonly ConcurrentDictionary<string, Counter> _counters = new();

        // Timer to periodically cleanup stale counters to avoid memory leak
        private readonly Timer _cleanupTimer;
        private bool _disposed;

        /// <summary>
        /// Initializes the filter with a cleanup timer.
        /// </summary>
        public HubMessageRateLimitFilter()
        {
            // start periodic cleanup
            _cleanupTimer = new Timer(DoCleanup, null, _cleanupInterval, _cleanupInterval);
        }

        private void DoCleanup(object? state)
        {
            try
            {
                var now = DateTime.UtcNow;
                foreach (var kv in _counters)
                {
                    var connectionId = kv.Key;
                    var counter = kv.Value;

                    // If last seen is older than stale threshold, remove
                    if ((now - counter.LastSeenUtc) > _staleThreshold)
                    {
                        _counters.TryRemove(connectionId, out _);
                    }
                }
            }
            catch
            {
                // Swallow to ensure timer keeps running; logging could be added here.
            }
        }

        /// <summary>
        /// Intercepts hub method invocations (client -> server). Enforces rate limit per connection.
        /// </summary>
        public async ValueTask<object?> InvokeMethodAsync(
            HubInvocationContext invocationContext,
            Func<HubInvocationContext, ValueTask<object?>> next)
        {
            var connectionId = invocationContext.Context.ConnectionId;
            var now = DateTime.UtcNow;

            var counter = _counters.GetOrAdd(connectionId, _ => new Counter
            {
                WindowStartUtc = now,
                Count = 0,
                LastSeenUtc = now
            });

            bool allowed = false;

            // update last seen and evaluate window / count under lock of the counter instance
            lock (counter)
            {
                counter.LastSeenUtc = now;

                if ((now - counter.WindowStartUtc) > _window)
                {
                    // new window
                    counter.WindowStartUtc = now;
                    counter.Count = 1;
                    allowed = true;
                }
                else
                {
                    if (counter.Count < _permitLimit)
                    {
                        counter.Count++;
                        allowed = true;
                    }
                    else
                    {
                        allowed = false;
                    }
                }
            }

            if (!allowed)
            {
                // When throttled, throw HubException so the client receives an error.
                // You could also choose to silently drop or delay messages depending on your UX.
                throw new HubException("Too many hub calls. Please slow down.");
            }

            // proceed with the invocation
            return await next(invocationContext);
        }

        /// <summary>
        /// Called when a new connection is established.
        /// We create/init the counter for the connection so it exists from the start.
        /// </summary>
        public async ValueTask OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, ValueTask> next)
        {
            var connectionId = context.Context.ConnectionId;
            var now = DateTime.UtcNow;

            // Initialize or reset counter for the new connection
            _counters.AddOrUpdate(
                connectionId,
                _ => new Counter { WindowStartUtc = now, Count = 0, LastSeenUtc = now },
                (_, existing) =>
                {
                    lock (existing)
                    {
                        existing.WindowStartUtc = now;
                        existing.Count = 0;
                        existing.LastSeenUtc = now;
                        return existing;
                    }
                });

            await next(context);
        }

        /// <summary>
        /// Called when the connection is disconnected. Remove the counter to free memory.
        /// </summary>
        public async ValueTask OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, ValueTask> next)
        {
            var connectionId = context.Context.ConnectionId;
            _counters.TryRemove(connectionId, out _);

            await next(context, exception);
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _cleanupTimer?.Dispose();
        }
    }
}
