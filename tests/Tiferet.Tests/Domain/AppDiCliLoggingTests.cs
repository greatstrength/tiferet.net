using Microsoft.Extensions.Logging;
using Tiferet.Domain;
using Tiferet.Mappers;
using Tiferet.Mappers.DI;
using Tiferet.Mappers.Cli;
using Tiferet.Mappers.App;
using Tiferet.Domain.App;
using Tiferet.Domain.Cli;
using Tiferet.Domain.DI;
using Tiferet.Domain.Logging;

namespace Tiferet.Tests.Domain;

// *** App Domain

public class AppRecordTests
{
    [Fact]
    public void GetService_ReturnsMatch()
    {
        var dep = new AppServiceDependencyConfiguration("svc1", "Asm", "Type");
        var app = new AppInterfaceConfiguration("id", "App", "Asm", "Type", Services: [dep]);
        Assert.Same(dep, app.GetService("svc1"));
    }

    [Fact]
    public void GetService_ReturnsNull_NoMatch()
    {
        var app = new AppInterfaceConfiguration("id", "App", "Asm", "Type");
        Assert.Null(app.GetService("missing"));
    }
}

public class AppInterfaceAggregateTests
{
    private static AppInterfaceAggregate Create() =>
        new(new AppInterfaceConfiguration("id", "App", "Asm", "Type"));

    [Fact]
    public void AddService_AddsToList()
    {
        var agg = Create();
        agg.AddService("svc1", "Asm", "Type");
        Assert.Single(agg.Services!);
    }

    [Fact]
    public void RemoveService_RemovesAndReturns()
    {
        var agg = Create();
        agg.AddService("svc1", "Asm", "Type");
        var removed = agg.RemoveService("svc1");
        Assert.NotNull(removed);
        Assert.Empty(agg.Services!);
    }

    [Fact]
    public void RemoveService_ReturnsNull_NotFound()
    {
        var agg = Create();
        Assert.Null(agg.RemoveService("missing"));
    }

    [Fact]
    public void SetConstants_MergesAndRemovesNulls()
    {
        var agg = new AppInterfaceAggregate(new AppInterfaceConfiguration("id", "App", "Asm", "Type",
            Constants: new Dictionary<string, string> { ["a"] = "1" }));
        agg.SetConstants(new() { ["b"] = "2", ["a"] = null });
        Assert.False(agg.Constants!.ContainsKey("a"));
        Assert.Equal("2", agg.Constants!["b"]);
    }
}

// *** DI Domain

public class DIRecordTests
{
    [Fact]
    public void GetDependency_ReturnsMatch()
    {
        var dep = new FlaggedDependencyConfiguration("prod", "Asm", "Type");
        var svc = new ServiceConfiguration("svc1", Dependencies: [dep]);
        Assert.Same(dep, svc.GetDependency("prod"));
    }

    [Fact]
    public void GetDependency_ReturnsNull_NoMatch()
    {
        var svc = new ServiceConfiguration("svc1");
        Assert.Null(svc.GetDependency("prod"));
    }

    [Fact]
    public void GetDependency_FlagPriority()
    {
        var dep1 = new FlaggedDependencyConfiguration("dev", "Asm1", "Type1");
        var dep2 = new FlaggedDependencyConfiguration("prod", "Asm2", "Type2");
        var svc = new ServiceConfiguration("svc1", Dependencies: [dep1, dep2]);
        Assert.Same(dep2, svc.GetDependency("prod", "dev"));
    }
}

public class ServiceConfigurationAggregateTests
{
    [Fact]
    public void SetDefaultType_Updates()
    {
        var agg = new ServiceConfigurationAggregate(new ServiceConfiguration("svc1"));
        agg.SetDefaultType("NewAsm", "NewType");
        Assert.Equal("NewAsm", agg.AssemblyName);
        Assert.Equal("NewType", agg.TypeName);
    }

    [Fact]
    public void SetDependency_AddsNew()
    {
        var agg = new ServiceConfigurationAggregate(new ServiceConfiguration("svc1"));
        agg.SetDependency("prod", "Asm", "Type");
        Assert.Single(agg.Dependencies!);
    }

    [Fact]
    public void SetDependency_ReplacesSameFlag()
    {
        var agg = new ServiceConfigurationAggregate(new ServiceConfiguration("svc1",
            Dependencies: [new FlaggedDependencyConfiguration("prod", "OldAsm", "OldType")]));
        agg.SetDependency("prod", "NewAsm", "NewType");
        Assert.Single(agg.Dependencies!);
        Assert.Equal("NewAsm", agg.Dependencies![0].AssemblyName);
    }

    [Fact]
    public void RemoveDependency_Removes()
    {
        var agg = new ServiceConfigurationAggregate(new ServiceConfiguration("svc1",
            Dependencies: [new FlaggedDependencyConfiguration("prod", "Asm", "Type")]));
        agg.RemoveDependency("prod");
        Assert.Empty(agg.Dependencies!);
    }
}

// *** CLI Domain

public class CliRecordTests
{
    [Fact]
    public void Create_DerivesIdFromGroupKeyAndKey()
    {
        var cmd = CliCommandAggregate.Create(name: "Add", key: "add", groupKey: "calc");
        Assert.Equal("calc.add", cmd.Id);
    }

    [Fact]
    public void Create_NormalizesHyphens()
    {
        var cmd = CliCommandAggregate.Create(name: "Add", key: "add-num", groupKey: "my-calc");
        Assert.Equal("my_calc.add_num", cmd.Id);
    }

    [Fact]
    public void HasArgument_ReturnsTrue()
    {
        var arg = new CliArgumentConfiguration(["-f", "--flag"]);
        var cmd = CliCommandAggregate.Create(name: "Cmd", key: "c", groupKey: "g", arguments: [arg]);
        Assert.True(cmd.HasArgument(["--flag"]));
    }

    [Fact]
    public void HasArgument_ReturnsFalse()
    {
        var cmd = CliCommandAggregate.Create(name: "Cmd", key: "c", groupKey: "g");
        Assert.False(cmd.HasArgument(["--missing"]));
    }
}

public class CliCommandAggregateTests
{
    [Fact]
    public void AddArgument_AddsToList()
    {
        var agg = CliCommandAggregate.Create(name: "Cmd", key: "c", groupKey: "g");
        agg.AddArgument(["a"], description: "First arg");
        Assert.Single(agg.Arguments!);
    }

    [Fact]
    public void Rename_Updates()
    {
        var agg = CliCommandAggregate.Create(name: "Old", key: "c", groupKey: "g");
        agg.Rename("New");
        Assert.Equal("New", agg.Name);
    }
}

// *** Logging Domain

public class LoggingRecordTests
{
    [Fact]
    public void Formatter_Construction()
    {
        var f = new FormatterConfiguration("fmt1", "Default", "%(message)s");
        Assert.Equal("fmt1", f.Id);
        Assert.Equal("%(message)s", f.Format);
    }

    [Fact]
    public void Handler_UsesLogLevel()
    {
        var h = new HandlerConfiguration("h1", "Console", "Asm", "Type", LogLevel.Information, "fmt1");
        Assert.Equal(LogLevel.Information, h.Level);
    }

    [Fact]
    public void Logger_Construction()
    {
        var l = new LoggerConfiguration("log1", "App LoggerConfiguration", LogLevel.Debug,
            HandlerIds: ["h1"], Propagate: false, IsRoot: true);
        Assert.True(l.IsRoot);
        Assert.Single(l.HandlerIds!);
    }
}
