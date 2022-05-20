namespace OneBarker.StructuredLogger.Managers;

/// <summary>
/// Options for the size limited log manager. 
/// </summary>
public sealed class SizeLimitedLogManagerOptions
{
    /// <summary>
    /// The default filename prefix.
    /// </summary>
    public const string DefaultFilenamePrefix = "application";

    /// <summary>
    /// The minimum number of logs to retain.
    /// </summary>
    public const int MinimumHistoryToKeep = 1;
    
    /// <summary>
    /// The maximum number of logs to retain.
    /// </summary>
    public const int MaximumHistoryToKeep = 100;
    
    /// <summary>
    /// The default number of logs to retain.
    /// </summary>
    public const int DefaultHistoryToKeep = 10;

    /// <summary>
    /// The minimum size limit (10 KiB).
    /// </summary>
    public const int MinimumSizeLimit = 10 << 10;

    /// <summary>
    /// The maximum size limit (100 MiB).
    /// </summary>
    public const int MaximumSizeLimit = 100 << 20;

    /// <summary>
    /// The default size limit (1 MiB).
    /// </summary>
    public const int DefaultSizeLimit = 1 << 20;
    
    /// <summary>
    /// The directory the log files are stored within (no default).
    /// </summary>
    public string LogFileDirectory { get; set; } = "";

    /// <summary>
    /// The filename prefix for log files.
    /// </summary>
    /// <remarks>
    /// The current log file will be in the format "{FilenamePrefix}.log".
    /// Historic log files will be in the format "{FilenamePrefix}-#.log".
    /// </remarks>
    public string FilenamePrefix { get; set; } = DefaultFilenamePrefix;

    /// <summary>
    /// The number of historic log files to keep.
    /// </summary>
    public int HistoryToKeep { get; set; } = DefaultHistoryToKeep;
    
    /// <summary>
    /// The size limit for log files.
    /// </summary>
    /// <remarks>
    /// The current log file will not exceed this size.
    /// </remarks>
    public int SizeLimit { get; set; } = DefaultSizeLimit;
}
