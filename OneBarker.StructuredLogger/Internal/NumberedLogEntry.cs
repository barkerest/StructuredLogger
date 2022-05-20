using Microsoft.Extensions.Logging;

namespace OneBarker.StructuredLogger.Internal;

// A helper class to enable sorting.
internal class NumberedLogEntry : ILogEntry, IComparable<NumberedLogEntry>
{
    public NumberedLogEntry(int index, ILogEntry source)
    {
        Index         = index;
        LogTime       = source.LogTime;
        LogLevel      = source.LogLevel;
        LogEvent      = source.LogEvent;
        LogText       = source.LogText;
        LogException  = source.LogException;
        LogParameters = source.LogParameters;
    }

    public int                                          Index         { get; }
    public DateTime                                     LogTime       { get; }
    public LogLevel                                     LogLevel      { get; }
    public int                                          LogEvent      { get; }
    public string                                       LogText       { get; }
    public string?                                      LogException  { get; }
    public IReadOnlyList<KeyValuePair<string, object?>> LogParameters { get; }

    public int CompareTo(NumberedLogEntry? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var timeComp = LogTime.CompareTo(other.LogTime);
        if (timeComp != 0) return timeComp;
        return Index.CompareTo(other.Index);
    }
}
