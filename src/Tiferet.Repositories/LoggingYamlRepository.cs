using Microsoft.Extensions.Logging;

using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers;
using Tiferet.Utilities;

namespace Tiferet.Repositories;

/// <summary>
/// YAML-backed repository for logging configurations (formatters, handlers, loggers).
/// Implements <see cref="ILoggingService"/> directly.
/// </summary>
public class LoggingYamlRepository : ILoggingService
{
    private readonly string _yamlFile;
    private readonly string _encoding;

    /// <summary>
    /// Initializes the logging YAML repository.
    /// </summary>
    /// <param name="loggingYamlFile">Path to the logging YAML config file.</param>
    /// <param name="encoding">File encoding (default: utf-8).</param>
    public LoggingYamlRepository(string loggingYamlFile, string encoding = "utf-8")
    {
        _yamlFile = loggingYamlFile;
        _encoding = encoding;
    }

    private Dictionary<object, object> LoadFull()
    {
        var loader = YamlLoader.ForReading(_yamlFile);
        var result = loader.Load();
        return result as Dictionary<object, object> ?? new Dictionary<object, object>();
    }

    private void SaveFull(Dictionary<object, object> data)
    {
        var loader = YamlLoader.ForWriting(_yamlFile);
        loader.Save(data);
    }

    /// <inheritdoc/>
    public (IReadOnlyList<FormatterAggregate> Formatters, IReadOnlyList<HandlerAggregate> Handlers, IReadOnlyList<LoggerAggregate> Loggers) ListAll()
    {
        var full = LoadFull();

        // Formatters.
        var fmtSection = YamlHelper.GetSection(full, "logging", "formatters");
        var formatters = fmtSection.Select(kv =>
            FormatterYamlObject.FromYaml(YamlHelper.ToStringDict(kv.Value), kv.Key).Map()).ToList();

        // Handlers.
        var hdlSection = YamlHelper.GetSection(full, "logging", "handlers");
        var handlers = hdlSection.Select(kv =>
            HandlerYamlObject.FromYaml(YamlHelper.ToStringDict(kv.Value), kv.Key).Map()).ToList();

        // Loggers.
        var logSection = YamlHelper.GetSection(full, "logging", "loggers");
        var loggers = logSection.Select(kv =>
            LoggerYamlObject.FromYaml(YamlHelper.ToStringDict(kv.Value), kv.Key).Map()).ToList();

        return (formatters, handlers, loggers);
    }

    /// <inheritdoc/>
    public void SaveFormatter(FormatterAggregate formatter)
    {
        var full = LoadFull();
        var dehydrated = FormatterYamlObject.FromAggregate(formatter).ToYamlDict();
        YamlHelper.SetNestedValue(full, dehydrated, "logging", "formatters", formatter.Domain.Id);
        SaveFull(full);
    }

    /// <inheritdoc/>
    public void SaveHandler(HandlerAggregate handler)
    {
        var full = LoadFull();
        var dehydrated = HandlerYamlObject.FromAggregate(handler).ToYamlDict();
        YamlHelper.SetNestedValue(full, dehydrated, "logging", "handlers", handler.Domain.Id);
        SaveFull(full);
    }

    /// <inheritdoc/>
    public void SaveLogger(LoggerAggregate logger)
    {
        var full = LoadFull();
        var dehydrated = LoggerYamlObject.FromAggregate(logger).ToYamlDict();
        YamlHelper.SetNestedValue(full, dehydrated, "logging", "loggers", logger.Domain.Id);
        SaveFull(full);
    }

    /// <inheritdoc/>
    public void DeleteFormatter(string id)
    {
        var full = LoadFull();
        YamlHelper.RemoveNestedValue(full, "logging", "formatters", id);
        SaveFull(full);
    }

    /// <inheritdoc/>
    public void DeleteHandler(string id)
    {
        var full = LoadFull();
        YamlHelper.RemoveNestedValue(full, "logging", "handlers", id);
        SaveFull(full);
    }

    /// <inheritdoc/>
    public void DeleteLogger(string id)
    {
        var full = LoadFull();
        YamlHelper.RemoveNestedValue(full, "logging", "loggers", id);
        SaveFull(full);
    }

}
