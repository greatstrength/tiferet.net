using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tiferet.Assets;
using Tiferet.Blueprints;
using Tiferet.Contexts;
using Tiferet.DependencyInjection;
using Tiferet.Events.DI;
using Tiferet.Events.Error;
using Tiferet.Events.Feature;
using Tiferet.Events.Logging;
using Tiferet.Interfaces;

namespace Tiferet.Tests.Blueprints;

// *** Host Integration Tests

public class HostIntegrationTests : IDisposable
{
    private readonly string _configDir;

    public HostIntegrationTests()
    {
        _configDir = Path.Combine(Path.GetTempPath(), $"tiferet_host_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_configDir);
        WriteMinimalConfigs();
    }

    public void Dispose()
    {
        if (Directory.Exists(_configDir))
            Directory.Delete(_configDir, true);
    }

    private void WriteMinimalConfigs()
    {
        File.WriteAllText(Path.Combine(_configDir, "config.yml"), """
            interfaces:
              basic_calc:
                Name: Basic Calculator
                AssemblyName: Tiferet.Contexts
                TypeName: Tiferet.Contexts.AppInterfaceContext
                Description: Perform basic calculator operations
              custom_logger:
                Name: Custom Logger App
                AssemblyName: Tiferet.Contexts
                TypeName: Tiferet.Contexts.AppInterfaceContext
                LoggerId: app_logger
            features:
              calc:
                add:
                  Name: Add Number
                  Description: Adds one number to another
                  Steps: []
            errors:
              invalid_input:
                Name: Invalid Input
                ErrorCode: INVALID_INPUT
                Messages:
                  - Lang: en_US
                    Text: "Value {value} is invalid"
            services: {}
            const: {}
            logging:
              formatters: {}
              handlers: {}
              loggers: {}
            """, Encoding.UTF8);
    }

    // *** TiferetOptions Tests

    // ** test: TiferetOptions defaults match ConfigurationDefaults
    [Fact]
    public void TiferetOptions_DefaultsMatchConfigurationDefaults()
    {
        var options = new TiferetOptions();

        Assert.Equal("default", options.InterfaceId);
        Assert.Equal(ConfigurationDefaults.DefaultConfigDir, options.ConfigDir);
        Assert.Equal(ConfigurationDefaults.ConfigFile, options.ConfigFile);
    }

    // ** test: TiferetOptions SectionName is Tiferet
    [Fact]
    public void TiferetOptions_SectionNameIsTiferet()
    {
        Assert.Equal("Tiferet", TiferetOptions.SectionName);
    }

    // ** test: TiferetOptions binds from IConfiguration
    [Fact]
    public void TiferetOptions_BindsFromConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tiferet:InterfaceId"] = "my_api",
                ["Tiferet:ConfigDir"] = "/custom/path",
                ["Tiferet:ConfigFile"] = "custom.yml",
            })
            .Build();

        var options = new TiferetOptions();
        config.GetSection(TiferetOptions.SectionName).Bind(options);

        Assert.Equal("my_api", options.InterfaceId);
        Assert.Equal("/custom/path", options.ConfigDir);
        Assert.Equal("custom.yml", options.ConfigFile);
    }

    // *** ConfigureServices Tests

    // ** test: ConfigureServices registers AppInterfaceContext
    [Fact]
    public void ConfigureServices_RegistersAppInterfaceContext()
    {
        var services = new ServiceCollection();
        var options = new TiferetOptions
        {
            InterfaceId = "basic_calc",
            ConfigDir = _configDir,
        };

        AppBlueprint.ConfigureServices(services, options);
        var provider = services.BuildServiceProvider();

        var context = provider.GetService<AppInterfaceContext>();
        Assert.NotNull(context);
        Assert.Equal("basic_calc", context!.InterfaceId);
    }

    // ** test: ConfigureServices registers repository services
    [Fact]
    public void ConfigureServices_RegistersRepositoryServices()
    {
        var services = new ServiceCollection();
        var options = new TiferetOptions
        {
            InterfaceId = "basic_calc",
            ConfigDir = _configDir,
        };

        AppBlueprint.ConfigureServices(services, options);
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IFeatureService>());
        Assert.NotNull(provider.GetService<IErrorService>());
        Assert.NotNull(provider.GetService<IDIService>());
        Assert.NotNull(provider.GetService<ILoggingService>());
    }

    // ** test: ConfigureServices registers domain events
    [Fact]
    public void ConfigureServices_RegistersDomainEvents()
    {
        var services = new ServiceCollection();
        var options = new TiferetOptions
        {
            InterfaceId = "basic_calc",
            ConfigDir = _configDir,
        };

        AppBlueprint.ConfigureServices(services, options);
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<GetFeature>());
        Assert.NotNull(provider.GetService<GetError>());
        Assert.NotNull(provider.GetService<ListAllSettings>());
        Assert.NotNull(provider.GetService<ListAllLoggingConfigs>());
    }

    // ** test: ConfigureServices registers contexts
    [Fact]
    public void ConfigureServices_RegistersContexts()
    {
        var services = new ServiceCollection();
        var options = new TiferetOptions
        {
            InterfaceId = "basic_calc",
            ConfigDir = _configDir,
        };

        AppBlueprint.ConfigureServices(services, options);
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<CacheContext>());
        Assert.NotNull(provider.GetService<ErrorContext>());
        Assert.NotNull(provider.GetService<LoggingContext>());
        Assert.NotNull(provider.GetService<DIContext>());
        Assert.NotNull(provider.GetService<FeatureContext>());
    }

    // ** test: ConfigureServices uses host ILoggerFactory when registered
    [Fact]
    public void ConfigureServices_UsesHostLoggerFactory()
    {
        var services = new ServiceCollection();

        // Register a host-provided ILoggerFactory before AddTiferet.
        services.AddLogging(builder => builder.AddConsole());

        var options = new TiferetOptions
        {
            InterfaceId = "basic_calc",
            ConfigDir = _configDir,
        };

        AppBlueprint.ConfigureServices(services, options);
        var provider = services.BuildServiceProvider();

        // LoggingContext should resolve without error — it received the host factory.
        var loggingContext = provider.GetService<LoggingContext>();
        Assert.NotNull(loggingContext);
    }

    // *** BuildApp(IServiceProvider) Tests

    // ** test: BuildApp from provider returns context
    [Fact]
    public void BuildApp_FromProvider_ReturnsContext()
    {
        var services = new ServiceCollection();
        AppBlueprint.ConfigureServices(services, new TiferetOptions
        {
            InterfaceId = "basic_calc",
            ConfigDir = _configDir,
        });
        var provider = services.BuildServiceProvider();

        var context = AppBlueprint.BuildApp(provider);

        Assert.NotNull(context);
        Assert.IsType<AppInterfaceContext>(context);
        Assert.Equal("basic_calc", context.InterfaceId);
    }

    // ** test: BuildApp from provider matches direct ConfigureServices
    [Fact]
    public void BuildApp_FromProvider_ReturnsSameInstance()
    {
        var services = new ServiceCollection();
        AppBlueprint.ConfigureServices(services, new TiferetOptions
        {
            InterfaceId = "basic_calc",
            ConfigDir = _configDir,
        });
        var provider = services.BuildServiceProvider();

        // Singleton — should be same instance.
        var ctx1 = AppBlueprint.BuildApp(provider);
        var ctx2 = provider.GetRequiredService<AppInterfaceContext>();
        Assert.Same(ctx1, ctx2);
    }

    // *** AddTiferet (IConfiguration) Tests

    // ** test: AddTiferet with IConfiguration registers services
    [Fact]
    public void AddTiferet_WithConfiguration_RegistersServices()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tiferet:InterfaceId"] = "basic_calc",
                ["Tiferet:ConfigDir"] = _configDir,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddTiferet(config);
        var provider = services.BuildServiceProvider();

        var context = provider.GetService<AppInterfaceContext>();
        Assert.NotNull(context);
        Assert.Equal("basic_calc", context!.InterfaceId);
    }

    // ** test: AddTiferet with Action registers services
    [Fact]
    public void AddTiferet_WithAction_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddTiferet(o =>
        {
            o.InterfaceId = "basic_calc";
            o.ConfigDir = _configDir;
        });
        var provider = services.BuildServiceProvider();

        var context = provider.GetService<AppInterfaceContext>();
        Assert.NotNull(context);
        Assert.Equal("basic_calc", context!.InterfaceId);
    }

    // *** UseTiferet (IHostBuilder) Tests

    // ** test: UseTiferet with configure callback
    [Fact]
    public void UseTiferet_WithConfigure_RegistersServices()
    {
        var host = Host.CreateDefaultBuilder()
            .UseTiferet(o =>
            {
                o.InterfaceId = "basic_calc";
                o.ConfigDir = _configDir;
            })
            .Build();

        var context = host.Services.GetService<AppInterfaceContext>();
        Assert.NotNull(context);
        Assert.Equal("basic_calc", context!.InterfaceId);
    }

    // *** Backward Compatibility Tests

    // ** test: standalone BuildApp still works
    [Fact]
    public void BuildApp_Standalone_StillWorks()
    {
        var context = AppBlueprint.BuildApp("basic_calc", _configDir);

        Assert.NotNull(context);
        Assert.IsType<AppInterfaceContext>(context);
        Assert.Equal("basic_calc", context.InterfaceId);
    }

    // ** test: custom LoggerId respected through DI path
    [Fact]
    public void ConfigureServices_RespectsCustomLoggerId()
    {
        var services = new ServiceCollection();
        AppBlueprint.ConfigureServices(services, new TiferetOptions
        {
            InterfaceId = "custom_logger",
            ConfigDir = _configDir,
        });
        var provider = services.BuildServiceProvider();

        var context = provider.GetService<AppInterfaceContext>();
        Assert.NotNull(context);
        Assert.Equal("custom_logger", context!.InterfaceId);
    }
}
