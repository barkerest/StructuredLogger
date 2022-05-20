namespace OneBarker.StructuredLogger.Managers;

/// <summary>
/// Options for the memory log manager.
/// </summary>
public sealed class MemoryLogManagerOptions
{
    /// <summary>
    /// The minimum record limit permitted for a memory manager.
    /// </summary>
    public const int MinimumRecordLimit = 1;
    
    /// <summary>
    /// The maximum record limit permitted for a memory manager.
    /// </summary>
    public const int MaximumRecordLimit = 1000000;

    /// <summary>
    /// The default record limit for a memory manager.
    /// </summary>
    public const int DefaultRecordLimit = 32000;

    /// <summary>
    /// The maximum number of records to retain in the memory manager.
    /// </summary>
    public int RecordLimit { get; set; } = DefaultRecordLimit;
}
