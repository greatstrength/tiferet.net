namespace Tiferet.Domain;

/// <summary>
/// Defines all structured error code constants used throughout the Tiferet framework.
/// </summary>
public static class ErrorCodes
{
    // Parameter and command errors
    public const string CommandParameterRequired = "COMMAND_PARAMETER_REQUIRED";
    public const string ParameterParsingFailed = "PARAMETER_PARSING_FAILED";
    public const string ParameterNotFound = "PARAMETER_NOT_FOUND";
    public const string RequestNotFound = "REQUEST_NOT_FOUND";

    // Import/dependency errors
    public const string ImportDependencyFailed = "IMPORT_DEPENDENCY_FAILED";
    public const string InvalidDependencyError = "INVALID_DEPENDENCY_ERROR";
    public const string DependencyTypeNotFound = "DEPENDENCY_TYPE_NOT_FOUND";

    // FeatureConfiguration errors
    public const string FeatureNotFound = "FEATURE_NOT_FOUND";
    public const string FeatureAlreadyExists = "FEATURE_ALREADY_EXISTS";
    public const string FeatureNameRequired = "FEATURE_NAME_REQUIRED";
    public const string InvalidFeatureAttribute = "INVALID_FEATURE_ATTRIBUTE";
    public const string FeatureCommandNotFound = "FEATURE_COMMAND_NOT_FOUND";
    public const string InvalidFeatureCommandAttribute = "INVALID_FEATURE_COMMAND_ATTRIBUTE";
    public const string FeatureCommandLoadingFailed = "FEATURE_COMMAND_LOADING_FAILED";

    // ErrorConfiguration domain errors
    public const string ErrorNotFound = "ERROR_NOT_FOUND";
    public const string ErrorAlreadyExists = "ERROR_ALREADY_EXISTS";
    public const string NoErrorMessages = "NO_ERROR_MESSAGES";

    // App interface errors
    public const string AppInterfaceNotFound = "APP_INTERFACE_NOT_FOUND";
    public const string InvalidAppInterfaceType = "INVALID_APP_INTERFACE_TYPE";
    public const string AppServiceImportFailed = "APP_SERVICE_IMPORT_FAILED";
    public const string AppServiceNotLoaded = "APP_SERVICE_NOT_LOADED";
    public const string AppRepositoryImportFailed = "APP_REPOSITORY_IMPORT_FAILED";

    // Model/aggregate errors
    public const string InvalidModelAttribute = "INVALID_MODEL_ATTRIBUTE";

    // Service configuration errors
    public const string InvalidServiceConfiguration = "INVALID_SERVICE_CONFIGURATION";
    public const string ServiceConfigurationNotFound = "SERVICE_CONFIGURATION_NOT_FOUND";
    public const string AttributeAlreadyExists = "ATTRIBUTE_ALREADY_EXISTS";
    public const string ConfigurationAlreadyExists = "CONFIGURATION_ALREADY_EXISTS";
    public const string InvalidFlaggedDependency = "INVALID_FLAGGED_DEPENDENCY";

    // CLI errors
    public const string CliCommandNotFound = "CLI_COMMAND_NOT_FOUND";
    public const string CliCommandAlreadyExists = "CLI_COMMAND_ALREADY_EXISTS";

    // Logging errors
    public const string LoggingConfigFailed = "LOGGING_CONFIG_FAILED";
    public const string LoggerCreationFailed = "LOGGER_CREATION_FAILED";

    // File errors
    public const string FileNotFound = "FILE_NOT_FOUND";
    public const string InvalidFile = "INVALID_FILE";
    public const string FileAlreadyOpen = "FILE_ALREADY_OPEN";
    public const string InvalidFileMode = "INVALID_FILE_MODE";
    public const string InvalidEncoding = "INVALID_ENCODING";

    // YAML errors
    public const string InvalidYamlFile = "INVALID_YAML_FILE";
    public const string YamlFileNotFound = "YAML_FILE_NOT_FOUND";
    public const string YamlFileLoadError = "YAML_FILE_LOAD_ERROR";
    public const string YamlFileSaveError = "YAML_FILE_SAVE_ERROR";

    // JSON errors
    public const string InvalidJsonFile = "INVALID_JSON_FILE";
    public const string JsonFileNotFound = "JSON_FILE_NOT_FOUND";
    public const string JsonFileLoadError = "JSON_FILE_LOAD_ERROR";
    public const string JsonFileSaveError = "JSON_FILE_SAVE_ERROR";
    public const string InvalidJsonPath = "INVALID_JSON_PATH";

    // CSV errors
    public const string CsvInvalidMode = "CSV_INVALID_MODE";
    public const string CsvHandleNotInitialized = "CSV_HANDLE_NOT_INITIALIZED";
    public const string CsvInvalidReadMode = "CSV_INVALID_READ_MODE";
    public const string CsvInvalidWriteMode = "CSV_INVALID_WRITE_MODE";
    public const string CsvFieldnamesRequired = "CSV_FIELDNAMES_REQUIRED";
    public const string CsvDictNoHeader = "CSV_DICT_NO_HEADER";

    // SQLite errors
    public const string SqliteConnAlreadyOpen = "SQLITE_CONN_ALREADY_OPEN";
    public const string SqliteInvalidMode = "SQLITE_INVALID_MODE";
    public const string SqliteFileNotFoundOrReadonly = "SQLITE_FILE_NOT_FOUND_OR_READONLY";
    public const string SqliteConnFailed = "SQLITE_CONN_FAILED";
    public const string SqliteBackupFailed = "SQLITE_BACKUP_FAILED";
    public const string SqliteConnNotInitialized = "SQLITE_CONN_NOT_INITIALIZED";

    // Config errors
    public const string UnsupportedConfigFileType = "UNSUPPORTED_CONFIG_FILE_TYPE";
    public const string AppError = "APP_ERROR";
    public const string ConfigFileNotFound = "CONFIG_FILE_NOT_FOUND";
}
