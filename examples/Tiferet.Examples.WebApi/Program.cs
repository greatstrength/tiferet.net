using Tiferet.Blueprints;
using Tiferet.DependencyInjection;

// Configuration directory relative to the binary output location.
var configDir = Path.Combine(AppContext.BaseDirectory, "app", "assets");

var builder = WebApplication.CreateBuilder(args);

// Configure Tiferet options.
var options = new TiferetOptions
{
    InterfaceId = "task_api",
    ConfigDir = configDir
};

// Register Tiferet services + health checks.
builder.Services.AddTiferet(opt =>
{
    opt.InterfaceId = options.InterfaceId;
    opt.ConfigDir = options.ConfigDir;
});

builder.Services.AddTiferetHealthChecks(options);

var app = builder.Build();

// Map all features as POST /api/{group}/{key} endpoints.
app.MapTiferetFeatures();

// Map health check endpoint.
app.MapHealthChecks("/health");

app.Run();
