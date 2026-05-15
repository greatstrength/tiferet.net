using Tiferet.Domain.Error;

namespace Tiferet.Mappers.Error;

/// <summary>Transfer object for <see cref="ErrorMessageConfiguration"/> in YAML configuration.</summary>
public class ErrorMessageYamlObject : TransferObject
{
    public string Lang { get; set; } = "";
    public string Text { get; set; } = "";

    public ErrorMessageConfiguration ToRecord() => new(Lang, Text);
    public static ErrorMessageYamlObject FromRecord(ErrorMessageConfiguration msg) => new() { Lang = msg.Lang, Text = msg.Text };

    public override Dictionary<string, object?> ToDictionary(string? role = null, Dictionary<string, object?>? overrides = null)
        => new() { ["Lang"] = Lang, ["Text"] = Text };

    public static ErrorMessageYamlObject FromYaml(Dictionary<string, object> data) => new()
    {
        Lang = data.TryGetValue("Lang", out var l) ? l?.ToString() ?? "" : "",
        Text = data.TryGetValue("Text", out var t) ? t?.ToString() ?? "" : "",
    };
}

/// <summary>Transfer object for <see cref="ErrorConfiguration"/> in YAML configuration.</summary>
public class ErrorYamlObject : TransferObject<ErrorConfiguration, ErrorAggregate>
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

    public override ErrorAggregate Map(Dictionary<string, object?>? overrides = null)
    {
        var messages = Messages?.Select(m => m.ToRecord()).ToList() as IReadOnlyList<ErrorMessageConfiguration>;
        return ErrorAggregate.Create(Id, Name, ErrorCode, Description, messages);
    }

    public static ErrorYamlObject FromAggregate(ErrorAggregate aggregate)
    {
        var d = aggregate.Domain;
        return new ErrorYamlObject
        {
            Id = d.Id, Name = d.Name, ErrorCode = d.ErrorCode, Description = d.Description,
            Messages = d.Messages?.Select(ErrorMessageYamlObject.FromRecord).ToList(),
        };
    }

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

    public Dictionary<object, object> ToYamlDict()
    {
        var result = new Dictionary<object, object> { ["Name"] = Name };
        if (Description is not null) result["Description"] = Description;
        if (Messages is not null && Messages.Count > 0)
            result["Messages"] = Messages.Select(m => (object)m.ToDictionary()!).ToList();
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
