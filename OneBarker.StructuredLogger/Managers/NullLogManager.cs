namespace OneBarker.StructuredLogger.Managers;

/// <summary>
/// A log manager that discards all entries.
/// </summary>
public class NullLogManager : ILogManager
{
    /// <inheritdoc />
    public void Dispose()
    {
        
    }

    /// <inheritdoc />
    public void QueueEntry(ILogEntry entry)
    {
        
    }

    /// <inheritdoc />
    public IEnumerable<ILogEntry> ReadEntries(DateTime? startTime = null, DateTime? endTime = null)
    {
        return Array.Empty<ILogEntry>();
    }
}
