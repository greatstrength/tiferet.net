using Tiferet.Domain;

namespace Tiferet.Mappers;

/// <summary>
/// Transfer object for <see cref="FlaggedDependency"/> in YAML configuration.
/// </summary>
public class FlaggedDependencyYamlObject : TransferObject
{
    public string Flag { get; set; } = "";
    public string AssemblyName { get; set; } = "";
    public string TypeName { get; set; } = "";
    public Dictionary<string, string>? Parameters { get; set; }

    public FlaggedDependency ToRecord() => new(Flag, AssemblyName, TypeName,
        Parameters is not null ? new Dictionary<string, string>(Parameters) : null);

    public static FlaggedDependencyYamlObject FromRecord(FlaggedDependency dep) => new()
    {
        Flag = dep.Flag, AssemblyName = dep.AssemblyName, TypeName = dep.TypeName,
        Parameters = dep.Parameters is not null ? new Dictionary<string, string>(dep.Parameters) : null,
    };

    /// <inheritdoc />
    public override Dictionary<string, object?> ToDictionary(string? role = null, Dictionary<string, object?>? overrides = null)
    {
        var result = new Dictionary<string, object?>
        {
            ["Flag"] = Flag,
            ["AssemblyName"] = AssemblyName,
            ["TypeName"] = TypeName,
        };
        if (Parameters is not null)
            result["Parameters"] = Parameters;
        return result;
    }

    public static FlaggedDependencyYamlObject FromYaml(Dictionary<string, object> data)
    {
        var obj = new FlaggedDependencyYamlObject
        {
            Flag = data.TryGetValue("Flag", out var f) ? f?.ToString() ?? "" : "",
            AssemblyName = data.TryGetValue("AssemblyName", out var an) ? an?.ToString() ?? "" : "",
            TypeName = data.TryGetValue("TypeName", out var tn) ? tn?.ToString() ?? "" : "",
        };
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
/// Transfer object for <see cref="ServiceConfiguration"/> in YAML configuration.
/// </summary>
public class ServiceConfigurationYamlObject : TransferObject<ServiceConfiguration, ServiceConfigurationAggregate>
{
    public string Id { get; set; } = "";
    public string? Name { get; set; }
    public string? AssemblyName { get; set; }
    public string? TypeName { get; set; }
    public Dictionary<string, string>? Parameters { get; set; }
    public List<FlaggedDependencyYamlObject>? Dependencies { get; set; }

    protected override Dictionary<string, RoleConfig> Roles { get; } = new()
    {
        [SerializationRoles.ToModel] = new() { Exclude = ["Dependencies", "Parameters"] },
        [SerializationRoles.ToDataYaml] = new() { Exclude = ["Id"] },
    };

    public override ServiceConfigurationAggregate Map(Dictionary<string, object?>? overrides = null)
    {
        IReadOnlyDictionary<string, string>? parameters = Parameters is not null
            ? new Dictionary<string, string>(Parameters) : null;
        var dependencies = Dependencies?.Select(d => d.ToRecord()).ToList() as IReadOnlyList<FlaggedDependency>;
        var config = new ServiceConfiguration(Id, Name, AssemblyName, TypeName, parameters, dependencies);
        return new ServiceConfigurationAggregate(config);
    }

    public static ServiceConfigurationYamlObject FromAggregate(ServiceConfigurationAggregate aggregate)
    {
        var d = aggregate.Domain;
        return new ServiceConfigurationYamlObject
        {
            Id = d.Id, Name = d.Name, AssemblyName = d.AssemblyName, TypeName = d.TypeName,
            Parameters = d.Parameters is not null ? new Dictionary<string, string>(d.Parameters) : null,
            Dependencies = d.Dependencies?.Select(FlaggedDependencyYamlObject.FromRecord).ToList(),
        };
    }

    public static ServiceConfigurationYamlObject FromYaml(Dictionary<string, object> data, string id)
    {
        var obj = new ServiceConfigurationYamlObject
        {
            Id = id,
            Name = data.TryGetValue("Name", out var n) ? n?.ToString() : null,
            AssemblyName = data.TryGetValue("AssemblyName", out var an) ? an?.ToString() : null,
            TypeName = data.TryGetValue("TypeName", out var tn) ? tn?.ToString() : null,
        };
        if (data.TryGetValue("Parameters", out var pObj))
        {
            var pd = ToStringDict(pObj);
            obj.Parameters = pd.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? "");
        }
        if (data.TryGetValue("Dependencies", out var dObj) && dObj is List<object> dList)
            obj.Dependencies = dList.Select(d => FlaggedDependencyYamlObject.FromYaml(ToStringDict(d))).ToList();
        return obj;
    }

    public Dictionary<object, object> ToYamlDict()
    {
        var result = new Dictionary<object, object>();
        if (Name is not null) result["Name"] = Name;
        if (AssemblyName is not null) result["AssemblyName"] = AssemblyName;
        if (TypeName is not null) result["TypeName"] = TypeName;
        if (Parameters is not null && Parameters.Count > 0)
            result["Parameters"] = Parameters.ToDictionary(kv => (object)kv.Key, kv => (object)kv.Value);
        if (Dependencies is not null && Dependencies.Count > 0)
            result["Dependencies"] = Dependencies.Select(d => (object)d.ToDictionary()!).ToList();
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
