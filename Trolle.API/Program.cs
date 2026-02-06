using Serilog;
using Trolle.API.Extensions;
using Trolle.Application;
using Trolle.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Presentation Layer
builder.Services.AddPresentation(builder.Configuration);

// Application Layer
builder.Services.AddApplication();

// Infrastructure Layer
builder.Services.AddInfrastructure();

// Serilog provider is configured in AddPresentation
builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline
await app.UsePresentation();

// Run the application
app.Run();