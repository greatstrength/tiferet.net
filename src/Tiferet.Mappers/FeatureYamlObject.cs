using Tiferet.Domain;

namespace Tiferet.Mappers;

/// <summary>
/// Transfer object for <see cref="FeatureEvent"/> in YAML configuration.
/// </summary>
public class FeatureEventYamlObject : TransferObject
{
    public string Name { get; set; } = "";
    public string ServiceId { get; set; } = "";
    public List<string>? Flags { get; set; }
    public Dictionary<string, string>? Parameters { get; set; }
    public string? DataKey { get; set; }
    public bool PassOnError { get; set; }
    public string? Condition { get; set; }

    public FeatureEvent ToRecord() => new(Name, ServiceId,
        Flags?.ToList(), Parameters is not null ? new Dictionary<string, string>(Parameters) : null,
        DataKey, PassOnError, Condition);

    public static FeatureEventYamlObject FromRecord(FeatureEvent ev) => new()
    {
        Name = ev.Name, ServiceId = ev.ServiceId,
        Flags = ev.Flags?.ToList(),
        Parameters = ev.Parameters is not null ? new Dictionary<string, string>(ev.Parameters) : null,
        DataKey = ev.DataKey, PassOnError = ev.PassOnError, Condition = ev.Condition,
    };

    public static FeatureEventYamlObject FromYaml(Dictionary<string, object> data)
    {
        var obj = new FeatureEventYamlObject
        {
            Name = data.TryGetValue("Name", out var n) ? n?.ToString() ?? "" : "",
            ServiceId = data.TryGetValue("ServiceId", out var s) ? s?.ToString() ?? "" : "",
            DataKey = data.TryGetValue("DataKey", out var dk) ? dk?.ToString() : null,
            Condition = data.TryGetValue("Condition", out var c) ? c?.ToString() : null,
        };
        if (data.TryGetValue("PassOnError", out var pe))
            obj.PassOnError = bool.TryParse(pe.ToString(), out var p) && p;
        if (data.TryGetValue("Flags", out var fObj) && fObj is List<object> fList)
            obj.Flags = fList.Select(f => f.ToString()!).ToList();
        if (data.TryGetValue("Parameters", out var pObj))
            obj.Parameters = ToStringStringDict(pObj);
        return obj;
    }

    private static Dictionary<string, string>? ToStringStringDict(object? raw)
    {
        if (raw is Dictionary<object, object> od)
            return od.ToDictionary(kv => kv.Key.ToString()!, kv => kv.Value?.ToString() ?? "");
        if (raw is Dictionary<string, object> sd)
            return sd.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? "");
        return null;
    }
}

/// <summary>
/// Transfer object for <see cref="Feature"/> in YAML configuration.
/// </summary>
public class FeatureYamlObject : TransferObject<Feature, FeatureAggregate>
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public List<string>? Flags { get; set; }
    public List<FeatureEventYamlObject>? Steps { get; set; }
    public Dictionary<string, string>? LogParams { get; set; }

    protected override Dictionary<string, RoleConfig> Roles { get; } = new()
    {
        [SerializationRoles.ToModel] = new() { Exclude = ["Steps", "Flags", "LogParams"] },
        [SerializationRoles.ToDataYaml] = new() { Exclude = ["Id"] },
    };

    public override FeatureAggregate Map(Dictionary<string, object?>? overrides = null)
    {
        var parts = Id.Split('.', 2);
        var steps = Steps?.Select(s => s.ToRecord()).ToList() as IReadOnlyList<FeatureEvent>;
        IReadOnlyDictionary<string, string>? logParams = LogParams is not null
            ? new Dictionary<string, string>(LogParams) : null;
        IReadOnlyList<string>? flags = Flags?.ToList();

        var feature = Feature.Create(Name, parts[0], parts.Length > 1 ? parts[1] : null, Id,
            Description, flags, steps, logParams);
        return new FeatureAggregate(feature);
    }

    public static FeatureYamlObject FromAggregate(FeatureAggregate aggregate)
    {
        var d = aggregate.Domain;
        return new FeatureYamlObject
        {
            Id = d.Id, Name = d.Name, Description = d.Description,
            Flags = d.Flags?.ToList(),
            Steps = d.Steps?.Select(FeatureEventYamlObject.FromRecord).ToList(),
            LogParams = d.LogParams is not null ? new Dictionary<string, string>(d.LogParams) : null,
        };
    }

    public static FeatureYamlObject FromYaml(Dictionary<string, object> data, string id)
    {
        var obj = new FeatureYamlObject
        {
            Id = id,
            Name = data.TryGetValue("Name", out var n) ? n?.ToString() ?? "" : "",
            Description = data.TryGetValue("Description", out var d) ? d?.ToString() : null,
        };
        if (data.TryGetValue("Flags", out var fObj) && fObj is List<object> fList)
            obj.Flags = fList.Select(f => f.ToString()!).ToList();
        if (data.TryGetValue("Steps", out var sObj) && sObj is List<object> sList)
            obj.Steps = sList.Select(s => FeatureEventYamlObject.FromYaml(ToStringDict(s))).ToList();
        if (data.TryGetValue("LogParams", out var lpObj))
        {
            var lpDict = ToStringDict(lpObj);
            obj.LogParams = lpDict.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? "");
        }
        return obj;
    }

    public Dictionary<object, object> ToYamlDict()
    {
        var result = new Dictionary<object, object> { ["Name"] = Name };
        if (Description is not null) result["Description"] = Description;
        if (Flags is not null && Flags.Count > 0) result["Flags"] = Flags.Cast<object>().ToList();
        if (Steps is not null && Steps.Count > 0)
            result["Steps"] = Steps.Select(s => (object)s.ToDictionary()!).ToList();
        if (LogParams is not null && LogParams.Count > 0)
            result["LogParams"] = LogParams.ToDictionary(kv => (object)kv.Key, kv => (object)kv.Value);
        return result;
    }

    private static Dictionary<string, object> ToStringDict(object raw)
    {
        if (raw is Dictionary<object, object> objDict)
            return objDict.ToDictionary(kv => kv.Key.ToString()!, kv => kv.Value);
        if (raw is Dictionary<string, object> strDict) return strDict;
        return new();
    }
}
