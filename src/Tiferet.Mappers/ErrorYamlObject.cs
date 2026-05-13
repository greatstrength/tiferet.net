using Tiferet.Domain;

namespace Tiferet.Mappers;

/// <summary>
/// Transfer object for <see cref="ErrorMessage"/> in YAML configuration.
/// Extends the non-generic <see cref="TransferObject"/> base (no aggregate).
/// </summary>
public class ErrorMessageYamlObject : TransferObject
{
    public string Lang { get; set; } = "";
    public string Text { get; set; } = "";

    /// <summary>Convert to domain record.</summary>
    public ErrorMessage ToRecord() => new(Lang, Text);

    /// <summary>Create from domain record.</summary>
    public static ErrorMessageYamlObject FromRecord(ErrorMessage msg)
        => new() { Lang = msg.Lang, Text = msg.Text };

    /// <inheritdoc />
    public override Dictionary<string, object?> ToDictionary(string? role = null, Dictionary<string, object?>? overrides = null)
        => new() { ["Lang"] = Lang, ["Text"] = Text };

    /// <summary>Create from a YAML data dictionary.</summary>
    public static ErrorMessageYamlObject FromYaml(Dictionary<string, object> data) => new()
    {
        Lang = data.TryGetValue("Lang", out var l) ? l?.ToString() ?? "" : "",
        Text = data.TryGetValue("Text", out var t) ? t?.ToString() ?? "" : "",
    };
}

/// <summary>
/// Transfer object for <see cref="Error"/> in YAML configuration.
/// </summary>
public class ErrorYamlObject : TransferObject<Error, ErrorAggregate>
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ErrorCode { get; set; }
    public string? Description { get; set; }
    public List<ErrorMessageYamlObject>? Messages { get; set; }

    protected override Dictionary<string, RoleConfig> Roles { get; } = new()
    {
        [SerializationRoles.ToModel] = new() { Exclude = ["Messages"] },
        [SerializationRoles.ToDataYaml] = new() { Exclude = ["Id", "ErrorCode"] },
    };

    /// <summary>Map to aggregate, converting nested messages.</summary>
    public override ErrorAggregate Map(Dictionary<string, object?>? overrides = null)
    {
        var messages = Messages?.Select(m => m.ToRecord()).ToList() as IReadOnlyList<ErrorMessage>;
        var error = Error.Create(Id, Name, ErrorCode, Description, messages);
        return new ErrorAggregate(error);
    }

    /// <summary>Create from aggregate, converting nested messages.</summary>
    public static ErrorYamlObject FromAggregate(ErrorAggregate aggregate)
    {
        var d = aggregate.Domain;
        return new ErrorYamlObject
        {
            Id = d.Id,
            Name = d.Name,
            ErrorCode = d.ErrorCode,
            Description = d.Description,
            Messages = d.Messages?.Select(ErrorMessageYamlObject.FromRecord).ToList(),
        };
    }

    /// <summary>Construct from a YAML data dictionary.</summary>
    public static ErrorYamlObject FromYaml(Dictionary<string, object> data, string id)
    {
        var obj = new ErrorYamlObject
        {
            Id = id,
            Name = data.TryGetValue("Name", out var n) ? n?.ToString() ?? "" : "",
            ErrorCode = data.TryGetValue("ErrorCode", out var ec) ? ec?.ToString() : null,
            Description = data.TryGetValue("Description", out var d) ? d?.ToString() : null,
        };

        if (data.TryGetValue("Messages", out var msgObj) && msgObj is List<object> msgList)
            obj.Messages = msgList.Select(m => ErrorMessageYamlObject.FromYaml(ToStringDict(m))).ToList();

        return obj;
    }

    /// <summary>Serialize to a YAML-compatible dictionary (excluding Id).</summary>
    public Dictionary<object, object> ToYamlDict()
    {
        var result = new Dictionary<object, object>();
        result["Name"] = Name;
        if (Description is not null) result["Description"] = Description;
        if (Messages is not null && Messages.Count > 0)
            result["Messages"] = Messages.Select(m => (object)m.ToDictionary()!).ToList();
        return result;
    }

    private static Dictionary<string, object> ToStringDict(object raw)
    {
        if (raw is Dictionary<object, object> objDict)
            return objDict.ToDictionary(kv => kv.Key.ToString()!, kv => kv.Value);
        if (raw is Dictionary<string, object> strDict)
            return strDict;
        return new();
    }
}
