using System.Text.RegularExpressions;
using OneBarker.StructuredLogger.Extensions;
using OneBarker.StructuredLogger.Internal;

namespace OneBarker.StructuredLogger.Managers;

/// <summary>
/// A daily log file manager.
/// </summary>
public class DailyLogManager : ILogManager, IWriteData
{
    private readonly string                    _filenamePrefix;
    private readonly Regex                     _filenamePattern;
    private readonly string                    _logDirectory;
    private readonly int                       _daysToKeep;
    private readonly LogQueue<DailyLogManager> _queue;

    private Stream? _currentStream;
    private int     _currentStreamVersion = 0;
    
    /// <summary>
    /// Create a new daily log file manager.
    /// </summary>
    /// <param name="options"></param>
    public DailyLogManager(DailyLogManagerOptions options)
    {
        _logDirectory    = this.CheckDirectory(options.LogFileDirectory);
        _filenamePrefix  = this.CreateSafeName(options.FilenamePrefix, DailyLogManagerOptions.DefaultFilenamePrefix);
        _filenamePattern = new Regex("^" + Regex.Escape(_filenamePrefix) + @"-(\d{8})\.log$");
        _daysToKeep      = options.DaysToKeep;
        if (_daysToKeep < DailyLogManagerOptions.MinimumDaysToKeep) _daysToKeep = DailyLogManagerOptions.MinimumDaysToKeep;
        if (_daysToKeep > DailyLogManagerOptions.MaximumDaysToKeep) _daysToKeep = DailyLogManagerOptions.MaximumDaysToKeep;

        _queue = new LogQueue<DailyLogManager>(this);
    }

    private int CurrentVersion 
        => this.DateToInt(DateTime.Today);

    private string Filename(int version) => $"{_logDirectory}/{_filenamePrefix}-{version:00000000}.log";

    private string[] FoundFiles
        => Directory.GetFiles(_logDirectory, $"{_filenamePrefix}-*.log", SearchOption.TopDirectoryOnly);
    
    private Stream CurrentStream
    {
        get
        {
            if (_currentStream is not null)
            {
                // return if current.
                if (_currentStreamVersion == CurrentVersion)
                    return _currentStream;
                
                // flush if not.
                _currentStream.Flush();
                _currentStream.Dispose();
                _currentStream = null;
            }

            // purge old logs.
            var oldestToKeep = this.DateToInt(DateTime.Today.AddDays(-_daysToKeep));
            foreach (var filePath in FoundFiles)
            {
                var match = _filenamePattern.Match(Path.GetFileName(filePath));
                if (!match.Success) continue;
                
                var ver = int.Parse(match.Groups[1].Value);
                if (ver < oldestToKeep)
                {
                    File.Delete(filePath);
                }
            }

            // open the new stream.
            _currentStreamVersion = CurrentVersion;
            var newFilename = Filename(_currentStreamVersion);
            _currentStream = File.Open(newFilename, FileMode.Append, FileAccess.Write, FileShare.Read);

            return _currentStream;
        }
    }

    void IWriteData.WriteData(byte[] data)
    {
        CurrentStream.Write(data, 0, data.Length);
    }

    void IWriteData.Flush()
    {
        if (_currentStream is not null)
        {
            _currentStream.Flush();
        }
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        _queue.Dispose();
        if (_currentStream is not null)
        {
            _currentStream.Flush();
            _currentStream.Dispose();
        }
    }

    /// <inheritdoc />
    public void QueueEntry(ILogEntry entry)
    {
        _queue.Queue(entry);
    }

    /// <inheritdoc />
    public IEnumerable<ILogEntry> ReadEntries(DateTime? startTime = null, DateTime? endTime = null)
    {
        _queue.Flush();
        var minVer = this.DateToInt(startTime.GetValueOrDefault());
        var maxVer = this.DateToInt(endTime ?? DateTime.Today);
        var data   = new List<NumberedLogEntry>();

        foreach (var filePath in FoundFiles)
        {
            var match = _filenamePattern.Match(Path.GetFileName(filePath));
            if (!match.Success) continue;

            var ver = int.Parse(match.Groups[1].Value);
            if (ver < minVer ||
                ver > maxVer) continue;
            
            foreach (var record in this.ReadFile(filePath))
            {
                if (startTime.HasValue &&
                    record.LogTime < startTime.GetValueOrDefault()) continue;
                if (endTime.HasValue &&
                    record.LogTime > endTime.GetValueOrDefault()) continue;
                data.Add(new NumberedLogEntry(data.Count, record));
            }
        }

        data.Sort();
        
        return ((IEnumerable<NumberedLogEntry>)data).Reverse();
    }
}
