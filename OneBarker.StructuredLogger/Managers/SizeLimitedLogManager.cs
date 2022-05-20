using System.Text.RegularExpressions;
using OneBarker.StructuredLogger.Extensions;
using OneBarker.StructuredLogger.Internal;

namespace OneBarker.StructuredLogger.Managers;

/// <summary>
/// A size limited log file manager.
/// </summary>
public class SizeLimitedLogManager : ILogManager, IWriteData
{
    private readonly string                          _filenamePrefix;
    private readonly Regex                           _filenamePattern;
    private readonly string                          _logDirectory;
    private readonly string                          _currentFile;
    private readonly int                             _sizeLimit;
    private readonly int                             _history;
    private readonly LogQueue<SizeLimitedLogManager> _queue;

    private Stream? _currentStream;
    
    /// <summary>
    /// Create a new size limited log file manager.
    /// </summary>
    /// <param name="options"></param>
    public SizeLimitedLogManager(SizeLimitedLogManagerOptions options)
    {
        _logDirectory    = this.CheckDirectory(options.LogFileDirectory);
        _filenamePrefix  = this.CreateSafeName(options.FilenamePrefix, DailyLogManagerOptions.DefaultFilenamePrefix);
        _filenamePattern = new Regex("^" + Regex.Escape(_filenamePrefix) + @"(?:-(\d+))?\.log$");

        _currentFile = _logDirectory + $"/{_filenamePrefix}.log";
        
        _sizeLimit       = options.SizeLimit;
        if (_sizeLimit < SizeLimitedLogManagerOptions.MinimumSizeLimit) _sizeLimit = SizeLimitedLogManagerOptions.MinimumSizeLimit;
        if (_sizeLimit > SizeLimitedLogManagerOptions.MaximumSizeLimit) _sizeLimit = SizeLimitedLogManagerOptions.MaximumSizeLimit;
        
        _history = options.HistoryToKeep;
        if (_history < SizeLimitedLogManagerOptions.MinimumHistoryToKeep) _history = SizeLimitedLogManagerOptions.MinimumHistoryToKeep;
        if (_history > SizeLimitedLogManagerOptions.MaximumHistoryToKeep) _history = SizeLimitedLogManagerOptions.MaximumHistoryToKeep;
        
        _queue = new LogQueue<SizeLimitedLogManager>(this);
    }


    private string Filename(int version) => _logDirectory + "/" + (version == 0 ? _filenamePrefix + ".log" : $"{_filenamePrefix}-{version}.log");

    private string[] FoundFiles
        => Directory.GetFiles(_logDirectory, $"{_filenamePrefix}*.log", SearchOption.TopDirectoryOnly);
    
    private Stream CurrentStream
    {
        get
        {
            return _currentStream ??= File.Open(_currentFile, FileMode.Append, FileAccess.Write, FileShare.Read);
        }
    }

    private Stream Rotate()
    {
        if (_currentStream is not null)
        {
            _currentStream.Flush();
            _currentStream.Dispose();
        }
        
        for (var i = _history - 1; i > 0; i--)
        {
            var j     = i - 1;
            var fileA = Filename(j);
            var fileB = Filename(i);
            if (File.Exists(fileB)) File.Delete(fileB);
            if (File.Exists(fileA)) File.Move(fileA, fileB);
        }

        return CurrentStream;
    }
    
    void IWriteData.WriteData(byte[] data)
    {
        if (data.Length >= _sizeLimit)  // if we get a massive record, it goes in it's own file.
        {
            // Close the current file.
            Rotate()
                // Write the massive log entry.
                .Write(data, 0, data.Length);
            
            // And then close the current file again since we can't fit anything else in with this entry.
            Rotate();
        }
        else
        {
            var size = File.Exists(_currentFile) ? new FileInfo(_currentFile).Length : 0;
            if (size + data.Length > _sizeLimit)
            {
                // Rotate and then write.
                Rotate().Write(data, 0, data.Length);
            }
            else
            {
                // Or append.
                CurrentStream.Write(data, 0, data.Length);
            }
        }
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
        var data   = new List<NumberedLogEntry>();

        foreach (var filePath in FoundFiles)
        {
            var match = _filenamePattern.Match(Path.GetFileName(filePath));
            if (!match.Success) continue;

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
