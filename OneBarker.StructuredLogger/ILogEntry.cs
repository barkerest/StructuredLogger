using Microsoft.Extensions.Logging;

namespace OneBarker.StructuredLogger;

/// <summary>
/// The interface for log entries.
/// </summary>
public interface ILogEntry
{
    /// <summary>
    /// The date and time of the entry.
    /// </summary>
    public DateTime LogTime { get; }
    
    /// <summary>
    /// The level of the entry.
    /// </summary>
    public LogLevel LogLevel { get; }
    
    /// <summary>
    /// The event ID for the entry.
    /// </summary>
    public int LogEvent { get; }
    
    /// <summary>
    /// The text for the entry.
    /// </summary>
    public string LogText { get; }
    
    /// <summary>
    /// Any exception details attached to the entry.
    /// </summary>
    public string? LogException { get; }
    
    /// <summary>
    /// Any additional parameters attached to the entry.
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, object?>> LogParameters { get; }
}

