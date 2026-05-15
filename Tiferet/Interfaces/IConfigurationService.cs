namespace Tiferet.Interfaces;

/// <summary>Service interface for loading and saving structured configuration data.</summary>
public interface IConfigurationService : IService
{
    /// <summary>Load configuration data.</summary>
    T Load<T>(Func<object, object>? startNode = null, Func<object, T>? dataFactory = null);

    /// <summary>Save configuration data.</summary>
    void Save<T>(T data, string? dataPath = null);
}
