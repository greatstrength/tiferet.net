using Tiferet.Domain;

namespace Tiferet.Mappers;

/// <summary>
/// Transfer object for <see cref="CliArgument"/> in YAML configuration.
/// </summary>
public class CliArgumentYamlObject : TransferObject
{
    public List<string> NameOrFlags { get; set; } = [];
    public string? Description { get; set; }
    public string Type { get; set; } = "String";
    public bool? Required { get; set; }
    public string? Default { get; set; }
    public List<string>? Choices { get; set; }
    public string? Nargs { get; set; }
    public string? Action { get; set; }

    public CliArgument ToRecord() => new(
        NameOrFlags,
        Description,
        Enum.TryParse<CliArgumentType>(Type, true, out var t) ? t : CliArgumentType.String,
        Required,
        Default,
        Choices?.ToList(),
        Nargs,
        Enum.TryParse<CliArgumentAction>(Action, true, out var a) ? a : null);

    public static CliArgumentYamlObject FromRecord(CliArgument arg) => new()
    {
        NameOrFlags = arg.NameOrFlags.ToList(),
        Description = arg.Description,
        Type = arg.Type.ToString(),
        Required = arg.Required,
        Default = arg.Default,
        Choices = arg.Choices?.ToList(),
        Nargs = arg.Nargs,
        Action = arg.Action?.ToString(),
    };

    /// <inheritdoc />
    public override Dictionary<string, object?> ToDictionary(string? role = null, Dictionary<string, object?>? overrides = null)
    {
        var result = new Dictionary<string, object?> { ["NameOrFlags"] = NameOrFlags, ["Type"] = Type };
        if (Description is not null) result["Description"] = Description;
        if (Required is not null) result["Required"] = Required;
        if (Default is not null) result["Default"] = Default;
        if (Choices is not null) result["Choices"] = Choices;
        if (Nargs is not null) result["Nargs"] = Nargs;
        if (Action is not null) result["Action"] = Action;
        return result;
    }

    public static CliArgumentYamlObject FromYaml(Dictionary<string, object> data)
    {
        var obj = new CliArgumentYamlObject
        {
            Description = data.TryGetValue("Description", out var d) ? d?.ToString() : null,
            Default = data.TryGetValue("Default", out var def) ? def?.ToString() : null,
            Nargs = data.TryGetValue("Nargs", out var na) ? na?.ToString() : null,
        };

        if (data.TryGetValue("NameOrFlags", out var nof))
        {
            obj.NameOrFlags = nof switch
            {
                List<object> list => list.Select(x => x.ToString()!).ToList(),
                string s => [s],
                _ => [],
            };
        }

        if (data.TryGetValue("Type", out var tObj))
            obj.Type = tObj?.ToString() ?? "String";
        if (data.TryGetValue("Required", out var rObj) && bool.TryParse(rObj.ToString(), out var r))
            obj.Required = r;
        if (data.TryGetValue("Choices", out var cObj) && cObj is List<object> cList)
            obj.Choices = cList.Select(c => c.ToString()!).ToList();
        if (data.TryGetValue("Action", out var aObj))
            obj.Action = aObj?.ToString();

        return obj;
    }
}

/// <summary>
/// Transfer object for <see cref="CliCommand"/> in YAML configuration.
/// </summary>
public class CliCommandYamlObject : TransferObject<CliCommand, CliCommandAggregate>
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public List<CliArgumentYamlObject>? Arguments { get; set; }

    protected override Dictionary<string, RoleConfig> Roles { get; } = new()
    {
        [SerializationRoles.ToModel] = new() { Exclude = ["Arguments"] },
        [SerializationRoles.ToDataYaml] = new() { Exclude = ["Id"] },
    };

    public override CliCommandAggregate Map(Dictionary<string, object?>? overrides = null)
    {
        var parts = Id.Split('.', 2);
        var arguments = Arguments?.Select(a => a.ToRecord()).ToList() as IReadOnlyList<CliArgument>;
        var cmd = CliCommand.Create(Name, parts.Length > 1 ? parts[1] : parts[0],
            parts[0], Id, Description, arguments);
        return new CliCommandAggregate(cmd);
    }

    public static CliCommandYamlObject FromAggregate(CliCommandAggregate aggregate)
    {
        var d = aggregate.Domain;
        return new CliCommandYamlObject
        {
            Id = d.Id, Name = d.Name, Description = d.Description,
            Arguments = d.Arguments?.Select(CliArgumentYamlObject.FromRecord).ToList(),
        };
    }

    public static CliCommandYamlObject FromYaml(Dictionary<string, object> data, string id)
    {
        var obj = new CliCommandYamlObject
        {
            Id = id,
            Name = data.TryGetValue("Name", out var n) ? n?.ToString() ?? "" : "",
            Description = data.TryGetValue("Description", out var d) ? d?.ToString() : null,
        };
        if (data.TryGetValue("Arguments", out var aObj) && aObj is List<object> aList)
            obj.Arguments = aList.Select(a => CliArgumentYamlObject.FromYaml(ToStringDict(a))).ToList();
        return obj;
    }

    public Dictionary<object, object> ToYamlDict()
    {
        var result = new Dictionary<object, object> { ["Name"] = Name };
        if (Description is not null) result["Description"] = Description;
        if (Arguments is not null && Arguments.Count > 0)
            result["Arguments"] = Arguments.Select(a => (object)a.ToDictionary()!).ToList();
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
