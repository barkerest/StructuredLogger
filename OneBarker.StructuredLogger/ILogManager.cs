namespace OneBarker.StructuredLogger;

/// <summary>
/// A backend manager for structured loggers to use.
/// </summary>
public interface ILogManager : IDisposable
{
    /// <summary>
    /// Queues an entry to be written to the log, must be thread safe.
    /// </summary>
    /// <param name="entry"></param>
    public void QueueEntry(ILogEntry entry);

    /// <summary>
    /// Reads entries from the log that fall within the specified time period (if any) returning them in descending chronological order (newest first).
    /// </summary>
    /// <param name="startTime">The oldest time for entries to be returned.</param>
    /// <param name="endTime">The newest time for entries to be returned.</param>
    /// <returns></returns>
    public IEnumerable<ILogEntry> ReadEntries(DateTime? startTime = null, DateTime? endTime = null);
}

