using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using System.Threading.RateLimiting;
using Trolle.API.Hubs;
using Trolle.API.Observability;
using Trolle.Application;
using Trolle.Infrastructure;
using Trolle.Infrastructure.Persistence;
using Trolle.Infrastructure.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Exporter;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// CORS options
builder.Services
    .AddOptions<CorsOptions>()
    .Bind(builder.Configuration.GetSection("Cors"))
    .Validate(o => o.AllowedOrigins.Length > 0, "Cors:AllowedOrigins must not be empty")
    .ValidateOnStart();

// Database options
builder.Services
    .AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection("Database"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), "Database:ConnectionString must not be empty")
    .ValidateOnStart();

// SignalR options
builder.Services
    .AddOptions<SignalROptions>()
    .Bind(builder.Configuration.GetSection("SignalR"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.BoardHubPath), "SignalR:BoardHubPath must not be empty")
    .ValidateOnStart();

// OpenTelemetry options
builder.Services
    .AddOptions<OpenTelemetryOptions>()
    .Bind(builder.Configuration.GetSection("OpenTelemetry"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint), "OpenTelemetry:Endpoint must not be empty")
    .ValidateOnStart();

// Swagger options (optional)
builder.Services.Configure<SwaggerOptions>(builder.Configuration.GetSection("Swagger"));

// OpenTelemetry Configuration
var openTelemetryOptions = builder.Configuration
    .GetSection("OpenTelemetry")
    .Get<OpenTelemetryOptions>()!;
var otlpEndpoint = openTelemetryOptions.Endpoint;
var serviceName = openTelemetryOptions.ServiceName;

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddPrometheusExporter()
        .AddOtlpExporter())
    .WithTracing(tracing => tracing
        .AddSource(serviceName)
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
            options.Protocol = OtlpExportProtocol.Grpc;
        }));

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", serviceName)
    .Enrich.With<ActivityEnricher>()
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.AddPolicy("read", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    options.AddPolicy("write", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    options.AddPolicy("signalr", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
});

// Swagger
var swaggerOptions = builder.Configuration.GetSection("Swagger").Get<SwaggerOptions>()!;
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(swaggerOptions.Version, new OpenApiInfo
    {
        Title = swaggerOptions.Title,
        Version = swaggerOptions.Version,
        Description = swaggerOptions.Description
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// CORS
var corsOptions = builder.Configuration.GetSection("Cors").Get<CorsOptions>()!;
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins(corsOptions.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// SignalR
builder.Services.AddSignalR();
builder.Services.AddSingleton<IHubFilter, HubExceptionFilter>();
builder.Services.AddSingleton<IHubFilter, HubMessageRateLimitFilter>();

// Clean Architecture Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        sp => sp.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString,
        name: "postgres",
        failureStatus: HealthStatus.Unhealthy);

var app = builder.Build();

// Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);

        var traceId = System.Diagnostics.Activity.Current?.TraceId.ToString()
                      ?? httpContext.Items["TraceId"]?.ToString()
                      ?? httpContext.TraceIdentifier;
        diagnosticContext.Set("TraceId", traceId);
    };
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Auto-migration
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TrolleDbContext>();

        var dbOptions = scope.ServiceProvider
            .GetRequiredService<IOptions<DatabaseOptions>>()
            .Value;

        for (var retry = 1; retry <= dbOptions.MaxRetries; retry++)
        {
            try
            {
                await db.Database.MigrateAsync();
                Console.WriteLine("Database migrated!");
                break;
            }
            catch
            {
                Console.WriteLine($"Waiting for DB... ({retry})");
                await Task.Delay(TimeSpan.FromSeconds(dbOptions.DelaySeconds));
            }
        }
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global exception handler middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = feature?.Error;

        var traceId = System.Diagnostics.Activity.Current?.TraceId.ToString()
                      ?? context.TraceIdentifier;

        Log.Error(exception,
            "Unhandled exception | TraceId={TraceId} | Path={Path}",
            traceId,
            feature?.Path);

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

// Enforce HTTPS and apply CORS policy
//app.UseHttpsRedirection();
app.UseCors("AllowClient");

// Rate limitting
app.UseRateLimiter();

// Authorization middleware
app.UseAuthorization();

// Map API controllers
app.MapControllers();

// Map SignalR hub using configured path
var signalROptions = app.Services
    .GetRequiredService<IOptions<SignalROptions>>()
    .Value;
app.MapHub<BoardHub>(signalROptions.BoardHubPath)
    .RequireRateLimiting("signalr");

// Map health checks
app.MapHealthChecks("/health");

// Map Prometheus scraping endpoint
app.MapPrometheusScrapingEndpoint();

// Application lifecycle events
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("Trolle.API started!");
    Log.Information("Trolle.API started!");
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("Trolle.API shutting down...");
    Log.Warning("Trolle.API shutting down...");
});

// Run the application
app.Run();