namespace Tiferet.Utilities.Json;

/// <summary>
/// Declares the JSON property naming convention for serialization and deserialization.
/// </summary>
public enum NamingConvention
{
    /// <summary>PascalCase — e.g., <c>FirstName</c>. Default .NET convention.</summary>
    PascalCase,

    /// <summary>camelCase — e.g., <c>firstName</c>. Common in JavaScript/REST APIs.</summary>
    CamelCase,

    /// <summary>snake_case — e.g., <c>first_name</c>. Common in Python/Ruby APIs.</summary>
    SnakeCase
}
