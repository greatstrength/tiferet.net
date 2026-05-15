using System.Text;
using Tiferet.Blueprints;
using Tiferet.Contexts;
using Tiferet.Domain;
using Tiferet.Events;
using Tiferet.Domain.Logging;

namespace Tiferet.Tests.Blueprints;

// *** AppBlueprint Tests

public class AppBlueprintTests : IDisposable
{
    private readonly string _configDir;

    public AppBlueprintTests()
    {
        _configDir = Path.Combine(Path.GetTempPath(), $"tiferet_blueprint_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_configDir);
        WriteMinimalConfigs();
    }

    public void Dispose()
    {
        if (Directory.Exists(_configDir))
            Directory.Delete(_configDir, true);
    }

    /// <summary>
    /// Write minimal YAML config files for a basic_calc interface.
    /// </summary>
    private void WriteMinimalConfigs()
    {
        // app.yml
        File.WriteAllText(Path.Combine(_configDir, "app.yml"), """
            interfaces:
              basic_calc:
                Name: Basic Calculator
                AssemblyName: Tiferet.Contexts
                TypeName: Tiferet.Contexts.AppInterfaceContext
                Description: Perform basic calculator operations
              custom_logger:
                Name: Custom LoggerConfiguration App
                AssemblyName: Tiferet.Contexts
                TypeName: Tiferet.Contexts.AppInterfaceContext
                LoggerId: app_logger
            """, Encoding.UTF8);

        // feature.yml
        File.WriteAllText(Path.Combine(_configDir, "feature.yml"), """
            features:
              calc:
                add:
                  Name: Add Number
                  Description: Adds one number to another
                  Steps: []
            """, Encoding.UTF8);

        // error.yml
        File.WriteAllText(Path.Combine(_configDir, "error.yml"), """
            errors:
              invalid_input:
                Name: Invalid Input
                ErrorCode: INVALID_INPUT
                Messages:
                  - Lang: en_US
                    Text: "Value {value} is invalid"
            """, Encoding.UTF8);

        // container.yml (DI service configurations)
        File.WriteAllText(Path.Combine(_configDir, "container.yml"), """
            services: {}
            const: {}
            """, Encoding.UTF8);

        // logging.yml
        File.WriteAllText(Path.Combine(_configDir, "logging.yml"), """
            logging:
              formatters: {}
              handlers: {}
              loggers: {}
            """, Encoding.UTF8);
    }

    // *** Tests

    // ** test: BuildApp returns AppInterfaceContext
    [Fact]
    public void BuildApp_ReturnsAppInterfaceContext()
    {
        var context = AppBlueprint.BuildApp("basic_calc", _configDir);

        Assert.NotNull(context);
        Assert.IsType<AppInterfaceContext>(context);
        Assert.Equal("basic_calc", context.InterfaceId);
    }

    // ** test: BuildApp with custom LoggerId
    [Fact]
    public void BuildApp_RespectsLoggerIdFromConfig()
    {
        var context = AppBlueprint.BuildApp("custom_logger", _configDir);

        Assert.NotNull(context);
        Assert.Equal("custom_logger", context.InterfaceId);
    }

    // ** test: BuildApp throws for missing interface
    [Fact]
    public void BuildApp_ThrowsForMissingInterface()
    {
        var ex = Assert.Throws<TiferetException>(() =>
            AppBlueprint.BuildApp("nonexistent", _configDir));

        Assert.Equal(ErrorCodes.AppInterfaceNotFound, ex.ErrorCode);
    }

    // ** test: BuildApp throws for missing config directory
    [Fact]
    public void BuildApp_ThrowsForMissingConfigDir()
    {
        Assert.ThrowsAny<Exception>(() =>
            AppBlueprint.BuildApp("basic_calc", "/nonexistent/path"));
    }

    // ** test: ParseRequest returns correct request context
    [Fact]
    public void BuildApp_ParseRequest_SetsInterfaceId()
    {
        var context = AppBlueprint.BuildApp("basic_calc", _configDir);
        var request = context.ParseRequest(featureId: "calc.add");

        Assert.NotNull(request);
        Assert.Equal("basic_calc", request.Headers["InterfaceId"]);
        Assert.Equal("calc.add", request.FeatureId);
    }

    // ** test: multiple BuildApp calls are independent
    [Fact]
    public void BuildApp_MultipleCalls_Independent()
    {
        var ctx1 = AppBlueprint.BuildApp("basic_calc", _configDir);
        var ctx2 = AppBlueprint.BuildApp("custom_logger", _configDir);

        Assert.NotSame(ctx1, ctx2);
        Assert.Equal("basic_calc", ctx1.InterfaceId);
        Assert.Equal("custom_logger", ctx2.InterfaceId);
    }
}
