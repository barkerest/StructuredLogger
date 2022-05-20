using OneBarker.StructuredLogger.Extensions;
using OneBarker.StructuredLogger.Internal;

namespace OneBarker.StructuredLogger.Managers;

/// <summary>
/// A simple memory log manager.
/// </summary>
public class MemoryLogManager : ILogManager
{
    
    private readonly List<LogEntry> _entries           = new();

    /// <summary>
    /// The maximum records that this memory manager will hold.
    /// </summary>
    public int RecordLimit { get; }

    /// <summary>
    /// Create a new memory log manager with the provided options.
    /// </summary>
    /// <param name="options"></param>
    public MemoryLogManager(MemoryLogManagerOptions options)
    {
        RecordLimit = options.RecordLimit;
        if (RecordLimit < MemoryLogManagerOptions.MinimumRecordLimit) RecordLimit = MemoryLogManagerOptions.MinimumRecordLimit;
        if (RecordLimit > MemoryLogManagerOptions.MaximumRecordLimit) RecordLimit = MemoryLogManagerOptions.MaximumRecordLimit;
        
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        
    }

    /// <inheritdoc />
    public void QueueEntry(ILogEntry entry)
    {
        lock (_entries)
        {
            if (_entries.Count == RecordLimit) _entries.RemoveAt(0);
            _entries.Add(LogEntry.Cast(entry));
        }
    }

    /// <inheritdoc />
    public IEnumerable<ILogEntry> ReadEntries(DateTime? startTime = null, DateTime? endTime = null)
    {
        IEnumerable<NumberedLogEntry> data;
        lock (_entries)
        {
            data = _entries.Select((x,i) => new NumberedLogEntry(i, x)).ToArray();
        }

        var st                       = startTime.GetValueOrDefault();
        var et                       = endTime.GetValueOrDefault();
        
        if (startTime.HasValue) data = data.Where(x => x.LogTime >= st);
        if (endTime.HasValue) data   = data.Where(x => x.LogTime <= et);

        data = data.OrderByDescending(x => this.TimeToLong(x.LogTime, x.Index));

        foreach (var entry in data)
        {
            yield return entry;
        }
    }
}
