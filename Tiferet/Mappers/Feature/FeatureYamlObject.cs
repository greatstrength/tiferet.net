using Tiferet.Domain.Feature;

namespace Tiferet.Mappers.Feature;

/// <summary>Transfer object for <see cref="FeatureEventConfiguration"/> in YAML configuration.</summary>
public class FeatureEventYamlObject : TransferObject
{
    public string Name { get; set; } = "";
    public string ServiceId { get; set; } = "";
    public List<string>? Flags { get; set; }
    public Dictionary<string, string>? Parameters { get; set; }
    public string? DataKey { get; set; }
    public bool PassOnError { get; set; }
    public string? Condition { get; set; }

    public FeatureEventConfiguration ToRecord() => new(Name, ServiceId,
        Flags?.ToList(), Parameters is not null ? new Dictionary<string, string>(Parameters) : null,
        DataKey, PassOnError, Condition);

    public static FeatureEventYamlObject FromRecord(FeatureEventConfiguration ev) => new()
    {
        Name = ev.Name, ServiceId = ev.ServiceId, Flags = ev.Flags?.ToList(),
        Parameters = ev.Parameters is not null ? new Dictionary<string, string>(ev.Parameters) : null,
        DataKey = ev.DataKey, PassOnError = ev.PassOnError, Condition = ev.Condition,
    };

    public override Dictionary<string, object?> ToDictionary(string? role = null, Dictionary<string, object?>? overrides = null)
    {
        var result = new Dictionary<string, object?> { ["Name"] = Name, ["ServiceId"] = ServiceId };
        if (Flags is not null) result["Flags"] = Flags;
        if (Parameters is not null) result["Parameters"] = Parameters;
        if (DataKey is not null) result["DataKey"] = DataKey;
        if (PassOnError) result["PassOnError"] = PassOnError;
        if (Condition is not null) result["Condition"] = Condition;
        return result;
    }

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

/// <summary>Transfer object for <see cref="FeatureConfiguration"/> in YAML configuration.</summary>
public class FeatureYamlObject : TransferObject<FeatureAggregate>
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
        var steps = Steps?.Select(s => s.ToRecord()).ToList() as IReadOnlyList<FeatureEventConfiguration>;
        IReadOnlyDictionary<string, string>? logParams = LogParams is not null
            ? new Dictionary<string, string>(LogParams) : null;
        IReadOnlyList<string>? flags = Flags?.ToList();

        return FeatureAggregate.Create(Name, parts[0], parts.Length > 1 ? parts[1] : null, Id,
            Description, flags, steps, logParams);
    }

    public static FeatureYamlObject FromAggregate(FeatureAggregate aggregate)
    {
        return new FeatureYamlObject
        {
            Id = aggregate.Id, Name = aggregate.Name, Description = aggregate.Description,
            Flags = aggregate.Flags?.ToList(),
            Steps = aggregate.Steps?.Select(FeatureEventYamlObject.FromRecord).ToList(),
            LogParams = aggregate.LogParams is not null ? new Dictionary<string, string>(aggregate.LogParams) : null,
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
