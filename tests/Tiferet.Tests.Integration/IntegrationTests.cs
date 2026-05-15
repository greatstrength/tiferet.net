using System.Text;
using Tiferet.Blueprints;
using Tiferet.Contexts;
using Tiferet.Events;
using Tiferet.Domain;
using Tiferet.Mappers;
using Tiferet.Repositories;
using Tiferet.Mappers.Feature;
using Tiferet.Mappers.App;
using Tiferet.Mappers.Error;
using Tiferet.Domain.Error;
using Tiferet.Domain.Feature;
using Tiferet.Domain.App;

namespace Tiferet.Tests.Integration;

// *** Integration test base fixture

/// <summary>
/// Shared fixture that writes a complete YAML config directory
/// referencing test events in the Tiferet.Tests.Integration assembly.
/// </summary>
public class IntegrationFixture : IDisposable
{
    public string ConfigDir { get; }

    public IntegrationFixture()
    {
        ConfigDir = Path.Combine(
            Path.GetTempPath(), $"tiferet_int_{Guid.NewGuid():N}");
        Directory.CreateDirectory(ConfigDir);
        WriteConfigs();
    }

    public void Dispose()
    {
        if (Directory.Exists(ConfigDir))
            Directory.Delete(ConfigDir, true);
    }

    private void WriteConfigs()
    {
        // app.yml
        File.WriteAllText(Path.Combine(ConfigDir, "app.yml"), """
            interfaces:
              test_app:
                Name: Test App
                AssemblyName: Tiferet.Tests.Integration
                TypeName: Tiferet.Tests.Integration.TestApp
            """, Encoding.UTF8);

        // container.yml — all test events
        File.WriteAllText(Path.Combine(ConfigDir, "container.yml"), """
            services:
              add_double_event:
                AssemblyName: Tiferet.Tests.Integration
                TypeName: Tiferet.Tests.Integration.AddDoubleEvent
              divide_double_event:
                AssemblyName: Tiferet.Tests.Integration
                TypeName: Tiferet.Tests.Integration.DivideDoubleEvent
              erroring_event:
                AssemblyName: Tiferet.Tests.Integration
                TypeName: Tiferet.Tests.Integration.ErroringEvent
              concat_event:
                AssemblyName: Tiferet.Tests.Integration
                TypeName: Tiferet.Tests.Integration.ConcatEvent
            const: {}
            """, Encoding.UTF8);

        // feature.yml
        File.WriteAllText(Path.Combine(ConfigDir, "feature.yml"), """
            features:
              math:
                add:
                  Name: Add Two Doubles
                  Description: Adds two double values
                  Steps:
                    - ServiceId: add_double_event
                      Name: Add A and B
                divide:
                  Name: Divide Two Doubles
                  Description: Divides A by B
                  Steps:
                    - ServiceId: divide_double_event
                      Name: Divide A by B
                with_preset:
                  Name: Add With Preset B
                  Description: Adds A and a preset B value
                  Steps:
                    - ServiceId: add_double_event
                      Name: Add A and preset B
                      Parameters:
                        B: '10'
                multi_step:
                  Name: Multi Step FeatureConfiguration
                  Description: Stores intermediate result then adds
                  Steps:
                    - ServiceId: concat_event
                      Name: Concat A and B
                      DataKey: concat_result
                    - ServiceId: add_double_event
                      Name: Add A and B
                error:
                  Name: ErrorConfiguration FeatureConfiguration
                  Description: FeatureConfiguration that always errors
                  Steps:
                    - ServiceId: erroring_event
                      Name: Always fails
            """, Encoding.UTF8);

        // error.yml — DIVISION_BY_ZERO defined; TEST_ERROR falls back to default
        File.WriteAllText(Path.Combine(ConfigDir, "error.yml"), """
            errors:
              DIVISION_BY_ZERO:
                Name: Division By Zero
                Messages:
                  - Lang: en_US
                    Text: 'Cannot divide by zero'
            """, Encoding.UTF8);

        // logging.yml
        File.WriteAllText(Path.Combine(ConfigDir, "logging.yml"), """
            logging:
              formatters: {}
              handlers: {}
              loggers: {}
            """, Encoding.UTF8);
    }
}

// *** App Bootstrap Tests

public class AppBootstrapTests : IDisposable
{
    private readonly IntegrationFixture _fixture = new();
    public void Dispose() => _fixture.Dispose();

    // ** test: BuildApp returns a wired AppInterfaceContext
    [Fact]
    public void BuildApp_ReturnsWiredContext()
    {
        var app = AppBlueprint.BuildApp("test_app", _fixture.ConfigDir);

        Assert.NotNull(app);
        Assert.IsType<AppInterfaceContext>(app);
        Assert.Equal("test_app", app.InterfaceId);
    }

    // ** test: Multiple calls return independent contexts
    [Fact]
    public void BuildApp_MultipleCalls_AreIndependent()
    {
        var app1 = AppBlueprint.BuildApp("test_app", _fixture.ConfigDir);
        var app2 = AppBlueprint.BuildApp("test_app", _fixture.ConfigDir);

        Assert.NotSame(app1, app2);
        Assert.Equal(app1.InterfaceId, app2.InterfaceId);
    }

    // ** test: BuildApp throws for unknown interface
    [Fact]
    public void BuildApp_UnknownInterface_Throws()
    {
        var ex = Assert.Throws<TiferetException>(() =>
            AppBlueprint.BuildApp("nonexistent", _fixture.ConfigDir));

        Assert.Equal(ErrorCodes.AppInterfaceNotFound, ex.ErrorCode);
    }
}

// *** FeatureConfiguration Execution Tests

public class FeatureExecutionTests : IDisposable
{
    private readonly IntegrationFixture _fixture = new();
    private readonly AppInterfaceContext _app;

    public FeatureExecutionTests()
    {
        _app = AppBlueprint.BuildApp("test_app", _fixture.ConfigDir);
    }

    public void Dispose() => _fixture.Dispose();

    // ** test: full pipeline add — type resolution → event execution → result
    [Fact]
    public void Run_Add_ReturnsSumViaFullPipeline()
    {
        var result = _app.Run("math.add", data: new()
        {
            ["a"] = "3",
            ["b"] = "4",
        });

        Assert.Equal(7.0, result);
    }

    // ** test: feature-level preset parameter overrides request data
    [Fact]
    public void Run_PresetParam_UsesConfiguredValue()
    {
        var result = _app.Run("math.with_preset", data: new()
        {
            ["a"] = "5",
        });

        // B is preset to '10' in feature.yml → 5 + 10 = 15
        Assert.Equal(15.0, result);
    }

    // ** test: multi-step feature accumulates data via DataKey
    [Fact]
    public void Run_MultiStep_AccumulatesDataKey()
    {
        var result = _app.Run("math.multi_step", data: new()
        {
            ["a"] = "3",
            ["b"] = "4",
        });

        // Step 1 (concat, DataKey = "concat_result") stores "3-4"
        // Step 2 (add) runs with same data: 3 + 4 = 7
        Assert.Equal(7.0, result);
    }

    // ** test: decimal values work end-to-end
    [Fact]
    public void Run_Add_Decimals_Works()
    {
        var result = _app.Run("math.add", data: new()
        {
            ["a"] = "1.5",
            ["b"] = "2.5",
        });

        Assert.Equal(4.0, result);
    }
}

// *** ErrorConfiguration Handling Tests

public class ErrorHandlingTests : IDisposable
{
    private readonly IntegrationFixture _fixture = new();
    private readonly AppInterfaceContext _app;

    public ErrorHandlingTests()
    {
        _app = AppBlueprint.BuildApp("test_app", _fixture.ConfigDir);
    }

    public void Dispose() => _fixture.Dispose();

    // ** test: domain exception propagates as TiferetApiException through pipeline
    [Fact]
    public void Run_DivideByZero_ThrowsTiferetApiException()
    {
        var ex = Assert.Throws<TiferetApiException>(() =>
            _app.Run("math.divide", data: new()
            {
                ["a"] = "8",
                ["b"] = "0",
            }));

        Assert.Equal("DIVISION_BY_ZERO", ex.ErrorCode);
    }

    // ** test: error message is formatted from error.yml definition
    [Fact]
    public void Run_DivideByZero_FormatsMessageFromYaml()
    {
        var ex = Assert.Throws<TiferetApiException>(() =>
            _app.Run("math.divide", data: new()
            {
                ["a"] = "5",
                ["b"] = "0",
            }));

        Assert.Equal("Cannot divide by zero", ex.Message);
    }

    // ** test: erroring event throws TiferetApiException
    [Fact]
    public void Run_ErroringEvent_ThrowsTiferetApiException()
    {
        var ex = Assert.Throws<TiferetApiException>(() =>
            _app.Run("math.error", data: new()
            {
                ["a"] = "trigger",
            }));

        // TEST_ERROR is not in error.yml; ErrorContext falls back gracefully
        Assert.NotNull(ex);
        Assert.NotEmpty(ex.ErrorCode);
    }

    // ** test: feature not found throws via error pipeline
    [Fact]
    public void Run_UnknownFeature_ThrowsTiferetApiException()
    {
        // FeatureConfiguration load raises TiferetException(FeatureNotFound) → HandleError → TiferetApiException
        var ex = Assert.Throws<TiferetApiException>(() =>
            _app.Run("math.nonexistent", data: new()));

        Assert.Equal(ErrorCodes.FeatureNotFound, ex.ErrorCode);
    }
}

// *** Repository Round-Trip Tests

public class RepositoryRoundTripTests : IDisposable
{
    private readonly string _tempDir;

    public RepositoryRoundTripTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tiferet_rr_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    // ** test: error YAML repository round-trip
    [Fact]
    public void ErrorRepo_SaveAndReload_Roundtrip()
    {
        var yamlFile = Path.Combine(_tempDir, "error.yml");
        File.WriteAllText(yamlFile, "errors: {}", Encoding.UTF8);

        var repo = new ErrorYamlRepository(yamlFile);
        repo.Save(ErrorAggregate.Create("test_error", "Test ErrorConfiguration",
            messages: [new ErrorMessageConfiguration("en_US", "Test message")]));

        var loaded = repo.Get("test_error");
        Assert.NotNull(loaded);
        Assert.Equal("Test ErrorConfiguration", loaded.Domain.Name);
        Assert.Single(loaded.Domain.Messages!);
        Assert.Equal("Test message", loaded.Domain.Messages![0].Text);
    }

    // ** test: feature YAML repository round-trip
    [Fact]
    public void FeatureRepo_SaveAndReload_Roundtrip()
    {
        var yamlFile = Path.Combine(_tempDir, "feature.yml");
        File.WriteAllText(yamlFile, "features: {}", Encoding.UTF8);

        var repo = new FeatureYamlRepository(yamlFile);
        repo.Save(FeatureAggregate.Create(
            name: "Add Number",
            groupId: "calc",
            featureKey: "add",
            steps: [new FeatureEventConfiguration("Add A and B", "add_number_event")]));

        var loaded = repo.Get("calc.add");
        Assert.NotNull(loaded);
        Assert.Equal("Add Number", loaded.Domain.Name);
        Assert.Single(loaded.Domain.Steps!);
        Assert.Equal("add_number_event", loaded.Domain.Steps![0].ServiceId);
    }

    // ** test: app interface YAML repository round-trip
    [Fact]
    public void AppRepo_SaveAndReload_Roundtrip()
    {
        var yamlFile = Path.Combine(_tempDir, "app.yml");
        File.WriteAllText(yamlFile, "interfaces: {}", Encoding.UTF8);

        var repo = new AppYamlRepository(yamlFile);
        var iface = new AppInterfaceConfiguration(
            Id: "my_app",
            Name: "My App",
            AssemblyName: "MyApp",
            TypeName: "MyApp.Program");
        repo.Save(new AppInterfaceAggregate(iface));

        var loaded = repo.Get("my_app");
        Assert.NotNull(loaded);
        Assert.Equal("My App", loaded.Domain.Name);
        Assert.Equal("MyApp", loaded.Domain.AssemblyName);
    }
}
