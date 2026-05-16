namespace Tiferet.Assets;

/// <summary>
/// Defines default configuration file names, directory paths, and repository type
/// mappings used by blueprints for reflective service resolution.
/// All values are plain strings — no direct type references to repositories.
/// </summary>
public static class ConfigurationDefaults
{
    // *** constants

    // ** constant: default_config_dir
    /// <summary>Default configuration directory for application assets.</summary>
    public const string DefaultConfigDir = "app/assets";

    // ** constant: config_file
    /// <summary>Default consolidated configuration file name.</summary>
    public const string ConfigFile = "config.yml";

    // ** constant: default_assembly
    /// <summary>Default assembly name for built-in repository types.</summary>
    public const string DefaultAssembly = "Tiferet";

    // ** constant: default_app_service_type
    /// <summary>Fully-qualified type name for the default app service repository.</summary>
    public const string DefaultAppServiceType = "Tiferet.Repositories.AppYamlRepository";

    // ** constant: default_feature_service_type
    /// <summary>Fully-qualified type name for the default feature service repository.</summary>
    public const string DefaultFeatureServiceType = "Tiferet.Repositories.FeatureYamlRepository";

    // ** constant: default_error_service_type
    /// <summary>Fully-qualified type name for the default error service repository.</summary>
    public const string DefaultErrorServiceType = "Tiferet.Repositories.ErrorYamlRepository";

    // ** constant: default_di_service_type
    /// <summary>Fully-qualified type name for the default DI service repository.</summary>
    public const string DefaultDIServiceType = "Tiferet.Repositories.DIYamlRepository";

    // ** constant: default_logging_service_type
    /// <summary>Fully-qualified type name for the default logging service repository.</summary>
    public const string DefaultLoggingServiceType = "Tiferet.Repositories.LoggingYamlRepository";

    // ** constant: default_cli_service_type
    /// <summary>Fully-qualified type name for the default CLI service repository.</summary>
    public const string DefaultCliServiceType = "Tiferet.Repositories.CliYamlRepository";
}
