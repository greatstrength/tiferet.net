using System.Text;

using Microsoft.Extensions.Logging;

using Tiferet.Domain;
using Tiferet.Mappers;
using Tiferet.Repositories;
using Tiferet.Mappers.Logging;
using Tiferet.Domain.Error;
using Tiferet.Domain.Logging;

namespace Tiferet.Tests.Repositories;

public class LoggingYamlRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _yamlFile;

    public LoggingYamlRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tiferet_repo_log_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _yamlFile = Path.Combine(_tempDir, "logging.yaml");
        File.WriteAllText(_yamlFile, """
            logging:
              formatters:
                simple:
                  Name: Simple FormatterConfiguration
                  Format: "{timestamp} {level} {message}"
              handlers:
                console:
                  Name: Console HandlerConfiguration
                  AssemblyName: MyApp
                  TypeName: MyApp.ConsoleHandler
                  Level: Information
                  FormatterId: simple
              loggers:
                default:
                  Name: Default LoggerConfiguration
                  Level: Debug
                  HandlerIds:
                    - console
            """, Encoding.UTF8);
    }

    public void Dispose() { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); }

    [Fact]
    public void ListAll_ReturnsAllThreeCategories()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        var (formatters, handlers, loggers) = repo.ListAll();
        Assert.Single(formatters);
        Assert.Single(handlers);
        Assert.Single(loggers);
    }

    [Fact]
    public void ListAll_HydratesFormatter()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        var (formatters, _, _) = repo.ListAll();
        var fmt = formatters[0];
        Assert.Equal("simple", fmt.Id);
        Assert.Equal("Simple FormatterConfiguration", fmt.Name);
        Assert.Contains("{message}", fmt.Format);
    }

    [Fact]
    public void ListAll_HydratesHandler()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        var (_, handlers, _) = repo.ListAll();
        var hdl = handlers[0];
        Assert.Equal("console", hdl.Id);
        Assert.Equal(LogLevel.Information, hdl.Level);
        Assert.Equal("simple", hdl.FormatterId);
    }

    [Fact]
    public void ListAll_HydratesLogger()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        var (_, _, loggers) = repo.ListAll();
        var logger = loggers[0];
        Assert.Equal("default", logger.Id);
        Assert.Equal(LogLevel.Debug, logger.Level);
        Assert.NotNull(logger.HandlerIds);
        Assert.Single(logger.HandlerIds);
        Assert.Equal("console", logger.HandlerIds[0]);
    }

    [Fact]
    public void SaveFormatter_RoundTrip()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        var fmt = new FormatterConfiguration("detailed", "Detailed FormatterConfiguration", "{timestamp} [{level}] {message}");
        repo.SaveFormatter(new FormatterAggregate(fmt));

        var (formatters, _, _) = repo.ListAll();
        Assert.Equal(2, formatters.Count);
        Assert.Contains(formatters, f => f.Id == "detailed");
    }

    [Fact]
    public void SaveHandler_RoundTrip()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        var hdl = new HandlerConfiguration("file", "File HandlerConfiguration", "MyApp", "MyApp.FileHandler",
            LogLevel.Warning, "simple", null, null, "app.log");
        repo.SaveHandler(new HandlerAggregate(hdl));

        var (_, handlers, _) = repo.ListAll();
        Assert.Equal(2, handlers.Count);
    }

    [Fact]
    public void SaveLogger_RoundTrip()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        var logger = new LoggerConfiguration("app", "App LoggerConfiguration", LogLevel.Error);
        repo.SaveLogger(new LoggerAggregate(logger));

        var (_, _, loggers) = repo.ListAll();
        Assert.Equal(2, loggers.Count);
    }

    [Fact]
    public void DeleteFormatter_Removes()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        repo.DeleteFormatter("simple");
        var (formatters, _, _) = repo.ListAll();
        Assert.Empty(formatters);
    }

    [Fact]
    public void DeleteHandler_Removes()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        repo.DeleteHandler("console");
        var (_, handlers, _) = repo.ListAll();
        Assert.Empty(handlers);
    }

    [Fact]
    public void DeleteLogger_Removes()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        repo.DeleteLogger("default");
        var (_, _, loggers) = repo.ListAll();
        Assert.Empty(loggers);
    }

    [Fact]
    public void Delete_IsIdempotent()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        repo.DeleteFormatter("nonexistent");
        repo.DeleteHandler("nonexistent");
        repo.DeleteLogger("nonexistent");
        var (formatters, handlers, loggers) = repo.ListAll();
        Assert.Single(formatters);
        Assert.Single(handlers);
        Assert.Single(loggers);
    }
}
