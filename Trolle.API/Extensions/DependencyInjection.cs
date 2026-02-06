using System.Net;
using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Trolle.API.Hubs;
using Trolle.API.Observability;
using Trolle.Infrastructure.Options;

namespace Trolle.API.Extensions;

/// <summary>
/// Provides extension methods to register presentation-layer services into the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds presentation-layer services (controllers, OpenTelemetry, logging, swagger, CORS, SignalR, health checks, rate limiting).
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions(configuration);
        services.AddOpenTelemetryProvider(configuration);
        services.AddSerilogProvider(configuration);
        services.AddCustomRateLimiter();
        services.AddCustomSwagger(configuration);
        services.AddCustomCors(configuration);
        services.AddCustomSignalR();
        services.AddHealthChecksProvider(configuration);

        services.AddControllers();
        services.AddEndpointsApiExplorer();

        return services;
    }

    /// <summary>
    /// Registers and validates strongly-typed options from configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    private static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection("Cors"))
            .Validate(o => o.AllowedOrigins.Length > 0, "Cors:AllowedOrigins must not be empty")
            .ValidateOnStart();

        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection("Database"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), "Database:ConnectionString must not be empty")
            .ValidateOnStart();

        services.AddOptions<SignalROptions>()
            .Bind(configuration.GetSection("SignalR"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.BoardHubPath), "SignalR:BoardHubPath must not be empty")
            .ValidateOnStart();

        services.AddOptions<OpenTelemetryOptions>()
            .Bind(configuration.GetSection("OpenTelemetry"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint), "OpenTelemetry:Endpoint must not be empty")
            .ValidateOnStart();

        services.Configure<SwaggerOptions>(configuration.GetSection("Swagger"));

        return services;
    }

    /// <summary>
    /// Configures OpenTelemetry metrics and tracing with OTLP exporter and Prometheus.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    private static IServiceCollection AddOpenTelemetryProvider(this IServiceCollection services, IConfiguration configuration)
    {
        var openTelemetryOptions = configuration.GetSection("OpenTelemetry").Get<OpenTelemetryOptions>()!;
        var otlpEndpoint = openTelemetryOptions.Endpoint;
        var serviceName = openTelemetryOptions.ServiceName;

        services.AddOpenTelemetry()
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

        return services;
    }

    /// <summary>
    /// Configures Serilog logger using settings from configuration and enrichers.
    /// </summary>
    /// <param name="services">The service collection (not modified but returned for chaining).</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    private static IServiceCollection AddSerilogProvider(this IServiceCollection services, IConfiguration configuration)
    {
        var openTelemetryOptions = configuration.GetSection("OpenTelemetry").Get<OpenTelemetryOptions>()!;
        var serviceName = openTelemetryOptions.ServiceName;

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", serviceName)
            .Enrich.With<ActivityEnricher>()
            .CreateLogger();

        return services;
    }

    /// <summary>
    /// Adds custom rate limiting policies for read, write and signalr traffic.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    private static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;

            options.AddPolicy("read", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: ip,
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 1200,
                        TokensPerPeriod = 600,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        QueueLimit = 50,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    });
            });

            options.AddPolicy("write", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: ip,
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 180,
                        TokensPerPeriod = 60,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        QueueLimit = 5,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    });
            });

            options.AddPolicy("signalr", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: ip,
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 40,
                        TokensPerPeriod = 10,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    });
            });
        });

        return services;
    }

    /// <summary>
    /// Adds Swagger/OpenAPI generation using configured options and XML comments.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    private static IServiceCollection AddCustomSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        var swaggerOptions = configuration.GetSection("Swagger").Get<SwaggerOptions>()!;
        services.AddSwaggerGen(options =>
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

        return services;
    }

    /// <summary>
    /// Configures CORS policy using allowed origins from configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    private static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOptions = configuration.GetSection("Cors").Get<CorsOptions>()!;
        services.AddCors(options =>
        {
            options.AddPolicy("AllowClient", policy =>
            {
                policy.WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Registers SignalR services and hub filters for exception handling and message rate limiting.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    private static IServiceCollection AddCustomSignalR(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IHubFilter, HubExceptionFilter>();


        return services;
    }

    /// <summary>
    /// Adds health checks and configures PostgreSQL readiness probe using database options.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    private static IServiceCollection AddHealthChecksProvider(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddNpgSql(
                sp => sp.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString,
                name: "postgres",
                failureStatus: HealthStatus.Unhealthy);

        return services;
    }
}