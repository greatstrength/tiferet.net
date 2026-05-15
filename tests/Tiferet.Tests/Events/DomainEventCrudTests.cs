using Microsoft.Extensions.Logging;
using Tiferet.Assets;
using Tiferet.Events;
using Tiferet.Domain;
using Tiferet.Events.Feature;
using Tiferet.Events.Error;
using Tiferet.Events.App;
using Tiferet.Events.DI;
using Tiferet.Events.Cli;
using Tiferet.Events.Logging;
using Tiferet.Interfaces;
using Tiferet.Mappers;
using Tiferet.Mappers.Feature;
using Tiferet.Mappers.DI;
using Tiferet.Mappers.Cli;
using Tiferet.Mappers.App;
using Tiferet.Mappers.Error;
using Tiferet.Mappers.Logging;
using Tiferet.Domain.Error;
using Tiferet.Domain.Feature;
using Tiferet.Domain.Cli;

namespace Tiferet.Tests.Events;

// *** Mock helpers

public class MockFeatureService : IFeatureService
{
    private readonly Dictionary<string, FeatureAggregate> _store = new();
    public bool Exists(string id) => _store.ContainsKey(id);
    public FeatureAggregate? Get(string id) => _store.TryGetValue(id, out var v) ? v : null;
    public IReadOnlyList<FeatureAggregate> List() => _store.Values.ToList();
    public void Save(FeatureAggregate entity) => _store[entity.Domain.Id] = entity;
    public void Delete(string id) => _store.Remove(id);
}

public class MockErrorService : IErrorService
{
    private readonly Dictionary<string, ErrorAggregate> _store = new();
    public bool Exists(string id) => _store.ContainsKey(id);
    public ErrorAggregate? Get(string id) => _store.TryGetValue(id, out var v) ? v : null;
    public IReadOnlyList<ErrorAggregate> List() => _store.Values.ToList();
    public void Save(ErrorAggregate entity) => _store[entity.Domain.Id] = entity;
    public void Delete(string id) => _store.Remove(id);
}

public class MockAppService : IAppService
{
    private readonly Dictionary<string, AppInterfaceAggregate> _store = new();
    public bool Exists(string id) => _store.ContainsKey(id);
    public AppInterfaceAggregate? Get(string id) => _store.TryGetValue(id, out var v) ? v : null;
    public IReadOnlyList<AppInterfaceAggregate> List() => _store.Values.ToList();
    public void Save(AppInterfaceAggregate entity) => _store[entity.Domain.Id] = entity;
    public void Delete(string id) => _store.Remove(id);
}

// *** FeatureConfiguration Event Tests

public class FeatureEventCrudTests
{
    [Fact]
    public void AddFeature_CreatesAndSaves()
    {
        var svc = new MockFeatureService();
        var evt = new AddFeature(svc);
        var result = evt.Execute(new("Add Number", "calc"));
        Assert.Equal("calc.add_number", result.Domain.Id);
        Assert.True(svc.Exists("calc.add_number"));
    }

    [Fact]
    public void AddFeature_ThrowsOnDuplicate()
    {
        var svc = new MockFeatureService();
        var evt = new AddFeature(svc);
        evt.Execute(new("Add", "calc"));
        var ex = Assert.Throws<TiferetException>(() => evt.Execute(new("Add", "calc")));
        Assert.Equal(ErrorCodes.FeatureAlreadyExists, ex.ErrorCode);
    }

    [Fact]
    public void GetFeature_ReturnsExisting()
    {
        var svc = new MockFeatureService();
        new AddFeature(svc).Execute(new("Add", "calc"));
        var result = new GetFeature(svc).Execute(new("calc.add"));
        Assert.Equal("Add", result.Domain.Name);
    }

    [Fact]
    public void GetFeature_ThrowsOnMissing()
    {
        var svc = new MockFeatureService();
        var ex = Assert.Throws<TiferetException>(
            () => new GetFeature(svc).Execute(new("missing")));
        Assert.Equal(ErrorCodes.FeatureNotFound, ex.ErrorCode);
    }

    [Fact]
    public void ListFeatures_FiltersByGroupId()
    {
        var svc = new MockFeatureService();
        new AddFeature(svc).Execute(new("Add", "calc"));
        new AddFeature(svc).Execute(new("Greet", "app"));
        var result = new ListFeatures(svc).Execute(new("calc"));
        Assert.Single(result);
        Assert.Equal("calc", result[0].Domain.GroupId);
    }

    [Fact]
    public void RemoveFeature_Deletes()
    {
        var svc = new MockFeatureService();
        new AddFeature(svc).Execute(new("Add", "calc"));
        new RemoveFeature(svc).Execute(new("calc.add"));
        Assert.False(svc.Exists("calc.add"));
    }

    [Fact]
    public void AddFeatureStep_AddsStep()
    {
        var svc = new MockFeatureService();
        new AddFeature(svc).Execute(new("Add", "calc"));
        new AddFeatureStep(svc).Execute(new("calc.add", "Step1", "svc1"));
        Assert.Single(svc.Get("calc.add")!.Domain.Steps!);
    }
}

// *** ErrorConfiguration Event Tests

public class ErrorEventCrudTests
{
    [Fact]
    public void AddError_CreatesWithMessage()
    {
        var svc = new MockErrorService();
        var result = new AddError(svc).Execute(new("inv_input", "Invalid Input", "Value must be a number"));
        Assert.Equal("INV_INPUT", result.Domain.ErrorCode);
        Assert.Single(result.Domain.Messages!);
    }

    [Fact]
    public void GetError_WithDefaults_FallsBack()
    {
        var svc = new MockErrorService();
        var result = new GetError(svc).Execute(new(ErrorCodes.FeatureNotFound, IncludeDefaults: true));
        Assert.Equal(ErrorCodes.FeatureNotFound, result.Domain.ErrorCode);
    }

    [Fact]
    public void GetError_WithoutDefaults_Throws()
    {
        var svc = new MockErrorService();
        var ex = Assert.Throws<TiferetException>(
            () => new GetError(svc).Execute(new(ErrorCodes.FeatureNotFound)));
        Assert.Equal(ErrorCodes.ErrorNotFound, ex.ErrorCode);
    }

    [Fact]
    public void RenameError_Updates()
    {
        var svc = new MockErrorService();
        new AddError(svc).Execute(new("err", "Old", "msg"));
        new RenameError(svc).Execute(new("err", "New"));
        Assert.Equal("New", svc.Get("err")!.Domain.Name);
    }

    [Fact]
    public void RemoveErrorMessage_ThrowsWhenNoneRemain()
    {
        var svc = new MockErrorService();
        new AddError(svc).Execute(new("err", "Err", "msg"));
        var ex = Assert.Throws<TiferetException>(
            () => new RemoveErrorMessage(svc).Execute(new("err")));
        Assert.Equal(ErrorCodes.NoErrorMessages, ex.ErrorCode);
    }
}

// *** App Event Tests

public class AppEventCrudTests
{
    [Fact]
    public void AddAppInterface_CreatesAndSaves()
    {
        var svc = new MockAppService();
        var result = new AddAppInterface(svc).Execute(
            new("basic", "Basic", "Asm", "Type"));
        Assert.Equal("basic", result.Domain.Id);
    }

    [Fact]
    public void GetAppInterface_ThrowsOnMissing()
    {
        var svc = new MockAppService();
        var ex = Assert.Throws<TiferetException>(
            () => new GetAppInterface(svc).Execute(new("missing")));
        Assert.Equal(ErrorCodes.AppInterfaceNotFound, ex.ErrorCode);
    }

    [Fact]
    public void SetServiceDependency_AddsService()
    {
        var svc = new MockAppService();
        new AddAppInterface(svc).Execute(new("app", "App", "Asm", "Type"));
        new Tiferet.Events.App.SetServiceDependency(svc).Execute(
            new("app", "svc1", "Asm", "SvcType"));
        Assert.Single(svc.Get("app")!.Domain.Services!);
    }
}

// *** DI Event Tests (representative)

public class DIEventCrudTests
{
    private class MockDIService : IDIService
    {
        private readonly Dictionary<string, ServiceConfigurationAggregate> _configs = new();
        private Dictionary<string, string> _constants = new();

        public bool ConfigurationExists(string id) => _configs.ContainsKey(id);
        public ServiceConfigurationAggregate? GetConfiguration(string id) =>
            _configs.TryGetValue(id, out var v) ? v : null;
        public (IReadOnlyList<ServiceConfigurationAggregate>, Dictionary<string, string>) ListAll() =>
            (_configs.Values.ToList(), _constants);
        public void SaveConfiguration(ServiceConfigurationAggregate c) => _configs[c.Domain.Id] = c;
        public void DeleteConfiguration(string id) => _configs.Remove(id);
        public void SaveConstants(Dictionary<string, string> c) => _constants = c;
    }

    [Fact]
    public void AddServiceConfiguration_Creates()
    {
        var svc = new MockDIService();
        var result = new AddServiceConfiguration(svc).Execute(
            new("svc1", AssemblyName: "Asm", TypeName: "Type"));
        Assert.Equal("svc1", result.Domain.Id);
    }

    [Fact]
    public void AddServiceConfiguration_ThrowsOnDuplicate()
    {
        var svc = new MockDIService();
        new AddServiceConfiguration(svc).Execute(new("svc1", AssemblyName: "Asm", TypeName: "Type"));
        var ex = Assert.Throws<TiferetException>(
            () => new AddServiceConfiguration(svc).Execute(new("svc1", AssemblyName: "Asm", TypeName: "Type")));
        Assert.Equal(ErrorCodes.ConfigurationAlreadyExists, ex.ErrorCode);
    }

    [Fact]
    public void SetServiceConstants_MergesAndRemoves()
    {
        var svc = new MockDIService();
        new SetServiceConstants(svc).Execute(new(new() { ["a"] = "1" }));
        var result = new SetServiceConstants(svc).Execute(new(new() { ["b"] = "2", ["a"] = null }));
        Assert.False(result.ContainsKey("a"));
        Assert.Equal("2", result["b"]);
    }
}

// *** CLI Event Tests (representative)

public class CliEventCrudTests
{
    private class MockCliService : ICliService
    {
        private readonly Dictionary<string, CliCommandAggregate> _store = new();
        public bool Exists(string id) => _store.ContainsKey(id);
        public CliCommandAggregate? Get(string id) => _store.TryGetValue(id, out var v) ? v : null;
        public IReadOnlyList<CliCommandAggregate> List() => _store.Values.ToList();
        public void Save(CliCommandAggregate entity) => _store[entity.Domain.Id] = entity;
        public void Delete(string id) => _store.Remove(id);
        public IReadOnlyList<CliArgumentConfiguration> GetParentArguments() => [];
    }

    [Fact]
    public void AddCliCommand_Creates()
    {
        var svc = new MockCliService();
        var result = new AddCliCommand(svc).Execute(new("Add", "add", "calc"));
        Assert.Equal("calc.add", result.Domain.Id);
    }

    [Fact]
    public void AddCliCommand_ThrowsOnDuplicate()
    {
        var svc = new MockCliService();
        new AddCliCommand(svc).Execute(new("Add", "add", "calc"));
        var ex = Assert.Throws<TiferetException>(
            () => new AddCliCommand(svc).Execute(new("Add", "add", "calc")));
        Assert.Equal(ErrorCodes.CliCommandAlreadyExists, ex.ErrorCode);
    }
}

// *** Logging Event Tests (representative)

public class LoggingEventCrudTests
{
    private class MockLoggingService : ILoggingService
    {
        public readonly Dictionary<string, FormatterAggregate> Formatters = new();
        public readonly Dictionary<string, HandlerAggregate> Handlers = new();
        public readonly Dictionary<string, LoggerAggregate> Loggers = new();

        public (IReadOnlyList<FormatterAggregate>, IReadOnlyList<HandlerAggregate>, IReadOnlyList<LoggerAggregate>) ListAll()
            => (Formatters.Values.ToList(), Handlers.Values.ToList(), Loggers.Values.ToList());
        public void SaveFormatter(FormatterAggregate f) => Formatters[f.Domain.Id] = f;
        public void SaveHandler(HandlerAggregate h) => Handlers[h.Domain.Id] = h;
        public void SaveLogger(LoggerAggregate l) => Loggers[l.Domain.Id] = l;
        public void DeleteFormatter(string id) => Formatters.Remove(id);
        public void DeleteHandler(string id) => Handlers.Remove(id);
        public void DeleteLogger(string id) => Loggers.Remove(id);
    }

    [Fact]
    public void AddFormatter_Creates()
    {
        var svc = new MockLoggingService();
        var result = new Tiferet.Events.Logging.AddFormatter(svc).Execute(
            new("fmt1", "Default", "%(msg)s"));
        Assert.Equal("fmt1", result.Domain.Id);
        Assert.Single(svc.Formatters);
    }

    [Fact]
    public void AddHandler_Creates()
    {
        var svc = new MockLoggingService();
        var result = new Tiferet.Events.Logging.AddHandler(svc).Execute(
            new("h1", "Console", "Asm", "Type", LogLevel.Information, "fmt1"));
        Assert.Equal(LogLevel.Information, result.Domain.Level);
    }

    [Fact]
    public void AddLogger_Creates()
    {
        var svc = new MockLoggingService();
        var result = new Tiferet.Events.Logging.AddLogger(svc).Execute(
            new("log1", "App", LogLevel.Debug, ["h1"]));
        Assert.Equal("log1", result.Domain.Id);
    }

    [Fact]
    public void RemoveFormatter_Deletes()
    {
        var svc = new MockLoggingService();
        new Tiferet.Events.Logging.AddFormatter(svc).Execute(new("fmt1", "F", "%(msg)s"));
        new Tiferet.Events.Logging.RemoveFormatter(svc).Execute(new("fmt1"));
        Assert.Empty(svc.Formatters);
    }
}
