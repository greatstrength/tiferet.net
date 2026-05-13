using System.Text;

using Microsoft.Extensions.Logging;

using Tiferet.Domain;
using Tiferet.Mappers;
using Tiferet.Repositories;

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
                  Name: Simple Formatter
                  Format: "{timestamp} {level} {message}"
              handlers:
                console:
                  Name: Console Handler
                  AssemblyName: MyApp
                  TypeName: MyApp.ConsoleHandler
                  Level: Information
                  FormatterId: simple
              loggers:
                default:
                  Name: Default Logger
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
        Assert.Equal("simple", fmt.Domain.Id);
        Assert.Equal("Simple Formatter", fmt.Domain.Name);
        Assert.Contains("{message}", fmt.Domain.Format);
    }

    [Fact]
    public void ListAll_HydratesHandler()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        var (_, handlers, _) = repo.ListAll();
        var hdl = handlers[0];
        Assert.Equal("console", hdl.Domain.Id);
        Assert.Equal(LogLevel.Information, hdl.Domain.Level);
        Assert.Equal("simple", hdl.Domain.FormatterId);
    }

    [Fact]
    public void ListAll_HydratesLogger()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        var (_, _, loggers) = repo.ListAll();
        var logger = loggers[0];
        Assert.Equal("default", logger.Domain.Id);
        Assert.Equal(LogLevel.Debug, logger.Domain.Level);
        Assert.NotNull(logger.Domain.HandlerIds);
        Assert.Single(logger.Domain.HandlerIds);
        Assert.Equal("console", logger.Domain.HandlerIds[0]);
    }

    [Fact]
    public void SaveFormatter_RoundTrip()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        var fmt = new Formatter("detailed", "Detailed Formatter", "{timestamp} [{level}] {message}");
        repo.SaveFormatter(new FormatterAggregate(fmt));

        var (formatters, _, _) = repo.ListAll();
        Assert.Equal(2, formatters.Count);
        Assert.Contains(formatters, f => f.Domain.Id == "detailed");
    }

    [Fact]
    public void SaveHandler_RoundTrip()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        var hdl = new Handler("file", "File Handler", "MyApp", "MyApp.FileHandler",
            LogLevel.Warning, "simple", null, null, "app.log");
        repo.SaveHandler(new HandlerAggregate(hdl));

        var (_, handlers, _) = repo.ListAll();
        Assert.Equal(2, handlers.Count);
    }

    [Fact]
    public void SaveLogger_RoundTrip()
    {
        var repo = new LoggingYamlRepository(_yamlFile);
        var logger = new Logger("app", "App Logger", LogLevel.Error);
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
