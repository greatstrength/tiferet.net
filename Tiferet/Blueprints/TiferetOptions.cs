using Tiferet.Assets;

namespace Tiferet.Blueprints;

/// <summary>
/// Configuration POCO that captures the user's intent for bootstrapping.
/// Lives in <c>Blueprints</c> because it is the blueprint's input —
/// the blueprint reads these values alongside <see cref="ConfigurationDefaults"/>
/// from Assets to produce service registrations.
/// Bindable from <c>IConfiguration</c> via the Options pattern.
/// </summary>
public class TiferetOptions
{
    // *** constants

    // ** constant: section_name
    /// <summary>The configuration section name used when binding from <c>IConfiguration</c>.</summary>
    public const string SectionName = "Tiferet";

    // *** properties

    // ** property: interface_id
    /// <summary>The application interface identifier to bootstrap.</summary>
    public string InterfaceId { get; set; } = "default";

    // ** property: config_dir
    /// <summary>The directory containing Tiferet configuration files.</summary>
    public string ConfigDir { get; set; } = ConfigurationDefaults.DefaultConfigDir;

    // ** property: config_file
    /// <summary>The consolidated configuration file name.</summary>
    public string ConfigFile { get; set; } = ConfigurationDefaults.ConfigFile;
}
