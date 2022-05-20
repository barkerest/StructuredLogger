namespace OneBarker.StructuredLogger.Managers;

/// <summary>
/// Options for the daily log manager. 
/// </summary>
public sealed class DailyLogManagerOptions
{
    /// <summary>
    /// The default filename prefix.
    /// </summary>
    public const string DefaultFilenamePrefix = "application";

    /// <summary>
    /// The minimum number of daily logs to retain.
    /// </summary>
    public const int MinimumDaysToKeep = 1;
    
    /// <summary>
    /// The maximum number of daily logs to retain.
    /// </summary>
    public const int MaximumDaysToKeep = 720;
    
    /// <summary>
    /// The default number of daily logs to retain.
    /// </summary>
    public const int DefaultDaysToKeep = 30;
    
    /// <summary>
    /// The directory the log files are stored within (no default).
    /// </summary>
    public string LogFileDirectory { get; set; } = "";

    /// <summary>
    /// The filename prefix for log files.
    /// </summary>
    /// <remarks>
    /// Full file names will have "-YYYYMMDD.log" appended to this value.
    /// </remarks>
    public string FilenamePrefix { get; set; } = DefaultFilenamePrefix;

    /// <summary>
    /// The number of daily log files to keep (default: 30).
    /// </summary>
    public int DaysToKeep { get; set; } = DefaultDaysToKeep;
}
