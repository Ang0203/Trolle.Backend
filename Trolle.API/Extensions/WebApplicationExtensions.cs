using System;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Serilog;
using Trolle.API.Hubs;
using Trolle.Infrastructure.Persistence;
using Trolle.Infrastructure.Options;

namespace Trolle.API.Extensions;

/// <summary>
/// Extension methods for configuring and running presentation-layer middleware and lifecycle events.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configure middleware, endpoints and lifecycle hooks for the presentation layer.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>A task that completes after configuration (used to await startup migrations in Development).</returns>
    public static async Task UsePresentation(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);

                var traceId = Activity.Current?.TraceId.ToString()
                              ?? httpContext.Items["TraceId"]?.ToString()
                              ?? httpContext.TraceIdentifier;
                diagnosticContext.Set("TraceId", traceId);
            };
        });

        if (app.Environment.IsDevelopment())
        {
            await app.ApplyMigrations();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCustomExceptionHandler();

        // app.UseHttpsRedirection();
        app.UseCors("AllowClient");
        app.UseRateLimiter();
        app.UseAuthorization();

        app.MapControllers();
        app.MapCustomHubs();
        app.MapHealthChecks("/health");
        app.MapPrometheusScrapingEndpoint();

        app.RegisterLifecycleEvents();
    }

    /// <summary>
    /// Apply EF Core migrations with retry logic based on <see cref="DatabaseOptions"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> used to create a service scope.</param>
    /// <returns>A task that completes when migrations succeed or retry attempts are exhausted.</returns>
    private static async Task ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrolleDbContext>();
        var dbOptions = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        for (var attempt = 1; attempt <= dbOptions.MaxRetries; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                Console.WriteLine("Database migrated!");
                break;
            }
            catch
            {
                Console.WriteLine($"Waiting for DB... ({attempt})");
                await Task.Delay(TimeSpan.FromSeconds(dbOptions.DelaySeconds));
            }
        }
    }

    /// <summary>
    /// Register a global exception handler that logs the error and returns a minimal JSON error response.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    private static void UseCustomExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = feature?.Error;

                var traceId = Activity.Current?.TraceId.ToString()
                              ?? context.TraceIdentifier;

                Log.Error(exception,
                    "Unhandled exception | TraceId={TraceId} | Path={Path}",
                    traceId,
                    feature?.Path);

                if (exception is Trolle.Application.Common.Exceptions.ConcurrencyException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        status = context.Response.StatusCode,
                        message = exception.Message,
                        traceId
                    });
                    return;
                }

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    status = context.Response.StatusCode,
                    message = "An unexpected error occurred",
                    traceId
                });
            });
        });
    }

    /// <summary>
    /// Map SignalR hubs using configured paths and apply SignalR-specific rate limiting.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    private static void MapCustomHubs(this WebApplication app)
    {
        var signalROptions = app.Services.GetRequiredService<IOptions<SignalROptions>>().Value;
        app.MapHub<BoardHub>(signalROptions.BoardHubPath)
            .RequireRateLimiting("signalr");
    }

    /// <summary>
    /// Register application lifecycle events for start and graceful shutdown logging.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> whose lifetime to observe.</param>
    private static void RegisterLifecycleEvents(this WebApplication app)
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            Console.WriteLine("Trolle.API started!");
            Log.Information("Trolle.API started!");
        });

        app.Lifetime.ApplicationStopping.Register(() =>
        {
            Console.WriteLine("Trolle.API shut down.");
            Log.Warning("Trolle.API shut down.");
        });
    }
}
