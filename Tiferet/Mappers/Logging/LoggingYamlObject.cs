using Microsoft.Extensions.Logging;
using Tiferet.Domain.Logging;

namespace Tiferet.Mappers.Logging;

/// <summary>Transfer object for <see cref="FormatterConfiguration"/> in YAML configuration.</summary>
public class FormatterYamlObject : TransferObject<FormatterAggregate>
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Format { get; set; } = "";
    public string? Description { get; set; }
    public string? DateFormat { get; set; }

    protected override Dictionary<string, RoleConfig> Roles { get; } = new()
    {
        ["ToDataYaml"] = new() { Exclude = ["Id"] },
    };

    public override FormatterAggregate Map(Dictionary<string, object?>? overrides = null)
        => new(new FormatterConfiguration(Id, Name, Format, Description, DateFormat));

    public static FormatterYamlObject FromAggregate(FormatterAggregate agg) => new()
    {
        Id = agg.Id, Name = agg.Name, Format = agg.Format,
        Description = agg.Description, DateFormat = agg.DateFormat,
    };

    public static FormatterYamlObject FromYaml(Dictionary<string, object> data, string id) => new()
    {
        Id = id,
        Name = data.TryGetValue("Name", out var n) ? n?.ToString() ?? "" : "",
        Format = data.TryGetValue("Format", out var f) ? f?.ToString() ?? "" : "",
        Description = data.TryGetValue("Description", out var d) ? d?.ToString() : null,
        DateFormat = data.TryGetValue("DateFormat", out var df) ? df?.ToString() : null,
    };

    public Dictionary<object, object> ToYamlDict()
    {
        var result = new Dictionary<object, object> { ["Name"] = Name, ["Format"] = Format };
        if (Description is not null) result["Description"] = Description;
        if (DateFormat is not null) result["DateFormat"] = DateFormat;
        return result;
    }
}

/// <summary>Transfer object for <see cref="HandlerConfiguration"/> in YAML configuration.</summary>
public class HandlerYamlObject : TransferObject<HandlerAggregate>
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string AssemblyName { get; set; } = "";
    public string TypeName { get; set; } = "";
    public string Level { get; set; } = "Information";
    public string FormatterId { get; set; } = "";
    public string? Description { get; set; }
    public string? Stream { get; set; }
    public string? Filename { get; set; }

    protected override Dictionary<string, RoleConfig> Roles { get; } = new()
    {
        ["ToDataYaml"] = new() { Exclude = ["Id"] },
    };

    public override HandlerAggregate Map(Dictionary<string, object?>? overrides = null)
    {
        var level = Enum.TryParse<LogLevel>(Level, true, out var lv) ? lv : LogLevel.Information;
        var record = new HandlerConfiguration(Id, Name, AssemblyName, TypeName, level,
            FormatterId, Description, Stream, Filename);
        return new HandlerAggregate(record);
    }

    public static HandlerYamlObject FromAggregate(HandlerAggregate agg)
    {
        return new HandlerYamlObject
        {
            Id = agg.Id, Name = agg.Name, AssemblyName = agg.AssemblyName, TypeName = agg.TypeName,
            Level = agg.Level.ToString(), FormatterId = agg.FormatterId,
            Description = agg.Description, Stream = agg.Stream, Filename = agg.Filename,
        };
    }

    public static HandlerYamlObject FromYaml(Dictionary<string, object> data, string id) => new()
    {
        Id = id,
        Name = data.TryGetValue("Name", out var n) ? n?.ToString() ?? "" : "",
        AssemblyName = data.TryGetValue("AssemblyName", out var an) ? an?.ToString() ?? "" : "",
        TypeName = data.TryGetValue("TypeName", out var tn) ? tn?.ToString() ?? "" : "",
        Level = data.TryGetValue("Level", out var lv) ? lv?.ToString() ?? "Information" : "Information",
        FormatterId = data.TryGetValue("FormatterId", out var fi) ? fi?.ToString() ?? "" : "",
        Description = data.TryGetValue("Description", out var d) ? d?.ToString() : null,
        Stream = data.TryGetValue("Stream", out var s) ? s?.ToString() : null,
        Filename = data.TryGetValue("Filename", out var fn) ? fn?.ToString() : null,
    };

    public Dictionary<object, object> ToYamlDict()
    {
        var result = new Dictionary<object, object>
        {
            ["Name"] = Name, ["AssemblyName"] = AssemblyName, ["TypeName"] = TypeName,
            ["Level"] = Level, ["FormatterId"] = FormatterId,
        };
        if (Description is not null) result["Description"] = Description;
        if (Stream is not null) result["Stream"] = Stream;
        if (Filename is not null) result["Filename"] = Filename;
        return result;
    }
}

/// <summary>Transfer object for <see cref="LoggerConfiguration"/> in YAML configuration.</summary>
public class LoggerYamlObject : TransferObject<LoggerAggregate>
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Level { get; set; } = "Information";
    public string? Description { get; set; }
    public List<string>? HandlerIds { get; set; }
    public bool Propagate { get; set; }
    public bool IsRoot { get; set; }

    protected override Dictionary<string, RoleConfig> Roles { get; } = new()
    {
        ["ToDataYaml"] = new() { Exclude = ["Id"] },
    };

    public override LoggerAggregate Map(Dictionary<string, object?>? overrides = null)
    {
        var level = Enum.TryParse<LogLevel>(Level, true, out var lv) ? lv : LogLevel.Information;
        var record = new LoggerConfiguration(Id, Name, level, Description,
            HandlerIds?.ToList(), Propagate, IsRoot);
        return new LoggerAggregate(record);
    }

    public static LoggerYamlObject FromAggregate(LoggerAggregate agg)
    {
        return new LoggerYamlObject
        {
            Id = agg.Id, Name = agg.Name, Level = agg.Level.ToString(),
            Description = agg.Description, HandlerIds = agg.HandlerIds?.ToList(),
            Propagate = agg.Propagate, IsRoot = agg.IsRoot,
        };
    }

    public static LoggerYamlObject FromYaml(Dictionary<string, object> data, string id)
    {
        var obj = new LoggerYamlObject
        {
            Id = id,
            Name = data.TryGetValue("Name", out var n) ? n?.ToString() ?? "" : "",
            Level = data.TryGetValue("Level", out var lv) ? lv?.ToString() ?? "Information" : "Information",
            Description = data.TryGetValue("Description", out var d) ? d?.ToString() : null,
        };
        if (data.TryGetValue("HandlerIds", out var hObj) && hObj is List<object> hList)
            obj.HandlerIds = hList.Select(h => h.ToString()!).ToList();
        if (data.TryGetValue("Propagate", out var pObj))
            obj.Propagate = bool.TryParse(pObj.ToString(), out var p) && p;
        if (data.TryGetValue("IsRoot", out var rObj))
            obj.IsRoot = bool.TryParse(rObj.ToString(), out var r) && r;
        return obj;
    }

    public Dictionary<object, object> ToYamlDict()
    {
        var result = new Dictionary<object, object> { ["Name"] = Name, ["Level"] = Level };
        if (Description is not null) result["Description"] = Description;
        if (HandlerIds is not null && HandlerIds.Count > 0) result["HandlerIds"] = HandlerIds.Cast<object>().ToList();
        if (Propagate) result["Propagate"] = Propagate;
        if (IsRoot) result["IsRoot"] = IsRoot;
        return result;
    }
}
