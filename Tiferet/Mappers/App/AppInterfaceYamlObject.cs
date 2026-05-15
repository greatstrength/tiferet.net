using Tiferet.Domain.App;

namespace Tiferet.Mappers.App;

/// <summary>Transfer object for <see cref="AppServiceDependencyConfiguration"/> in YAML configuration.</summary>
public class AppServiceDependencyYamlObject : TransferObject
{
    public string ServiceId { get; set; } = "";
    public string AssemblyName { get; set; } = "";
    public string TypeName { get; set; } = "";
    public Dictionary<string, string>? Parameters { get; set; }

    public AppServiceDependencyConfiguration ToRecord() => new(ServiceId, AssemblyName, TypeName,
        Parameters is not null ? new Dictionary<string, string>(Parameters) : null);

    public static AppServiceDependencyYamlObject FromRecord(AppServiceDependencyConfiguration svc) => new()
    {
        ServiceId = svc.ServiceId,
        AssemblyName = svc.AssemblyName,
        TypeName = svc.TypeName,
        Parameters = svc.Parameters is not null ? new Dictionary<string, string>(svc.Parameters) : null,
    };

    public override Dictionary<string, object?> ToDictionary(string? role = null, Dictionary<string, object?>? overrides = null)
    {
        var result = new Dictionary<string, object?>
        {
            ["ServiceId"] = ServiceId,
            ["AssemblyName"] = AssemblyName,
            ["TypeName"] = TypeName,
        };
        if (Parameters is not null) result["Parameters"] = Parameters;
        return result;
    }

    public static AppServiceDependencyYamlObject FromYaml(Dictionary<string, object> data)
    {
        var obj = new AppServiceDependencyYamlObject
        {
            ServiceId = data.TryGetValue("ServiceId", out var sid) ? sid?.ToString() ?? "" : "",
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

/// <summary>Transfer object for <see cref="AppInterfaceConfiguration"/> in YAML configuration.</summary>
public class AppInterfaceYamlObject : TransferObject<AppInterfaceConfiguration, AppInterfaceAggregate>
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string AssemblyName { get; set; } = "";
    public string TypeName { get; set; } = "";
    public string? Description { get; set; }
    public string LoggerId { get; set; } = "default";
    public List<string>? Flags { get; set; }
    public List<AppServiceDependencyYamlObject>? Services { get; set; }
    public Dictionary<string, string>? Constants { get; set; }

    protected override Dictionary<string, RoleConfig> Roles { get; } = new()
    {
        [SerializationRoles.ToModel] = new() { Exclude = ["Services", "Constants", "Flags"] },
        [SerializationRoles.ToDataYaml] = new() { Exclude = ["Id"] },
    };

    public override AppInterfaceAggregate Map(Dictionary<string, object?>? overrides = null)
    {
        var services = Services?.Select(s => s.ToRecord()).ToList() as IReadOnlyList<AppServiceDependencyConfiguration>;
        IReadOnlyDictionary<string, string>? constants = Constants is not null
            ? new Dictionary<string, string>(Constants) : null;
        IReadOnlyList<string>? flags = Flags?.ToList();

        var app = new AppInterfaceConfiguration(Id, Name, AssemblyName, TypeName, Description,
            LoggerId, flags, services, constants);
        return new AppInterfaceAggregate(app);
    }

    public static AppInterfaceYamlObject FromAggregate(AppInterfaceAggregate aggregate)
    {
        var d = aggregate.Domain;
        return new AppInterfaceYamlObject
        {
            Id = d.Id, Name = d.Name, AssemblyName = d.AssemblyName, TypeName = d.TypeName,
            Description = d.Description, LoggerId = d.LoggerId,
            Flags = d.Flags?.ToList(),
            Services = d.Services?.Select(AppServiceDependencyYamlObject.FromRecord).ToList(),
            Constants = d.Constants is not null ? new Dictionary<string, string>(d.Constants) : null,
        };
    }

    public static AppInterfaceYamlObject FromYaml(Dictionary<string, object> data, string id)
    {
        var obj = new AppInterfaceYamlObject
        {
            Id = id,
            Name = data.TryGetValue("Name", out var n) ? n?.ToString() ?? "" : "",
            AssemblyName = data.TryGetValue("AssemblyName", out var an) ? an?.ToString() ?? "" : "",
            TypeName = data.TryGetValue("TypeName", out var tn) ? tn?.ToString() ?? "" : "",
            Description = data.TryGetValue("Description", out var d) ? d?.ToString() : null,
            LoggerId = data.TryGetValue("LoggerId", out var li) ? li?.ToString() ?? "default" : "default",
        };
        if (data.TryGetValue("Flags", out var fObj) && fObj is List<object> fList)
            obj.Flags = fList.Select(f => f.ToString()!).ToList();
        if (data.TryGetValue("Services", out var sObj) && sObj is List<object> sList)
            obj.Services = sList.Select(s => AppServiceDependencyYamlObject.FromYaml(ToStringDict(s))).ToList();
        if (data.TryGetValue("Constants", out var cObj))
        {
            var cd = ToStringDict(cObj);
            obj.Constants = cd.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? "");
        }
        return obj;
    }

    public Dictionary<object, object> ToYamlDict()
    {
        var result = new Dictionary<object, object> { ["Name"] = Name, ["AssemblyName"] = AssemblyName, ["TypeName"] = TypeName };
        if (Description is not null) result["Description"] = Description;
        if (LoggerId != "default") result["LoggerId"] = LoggerId;
        if (Flags is not null && Flags.Count > 0) result["Flags"] = Flags.Cast<object>().ToList();
        if (Services is not null && Services.Count > 0)
            result["Services"] = Services.Select(s => (object)s.ToDictionary()!).ToList();
        if (Constants is not null && Constants.Count > 0)
            result["Constants"] = Constants.ToDictionary(kv => (object)kv.Key, kv => (object)kv.Value);
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
