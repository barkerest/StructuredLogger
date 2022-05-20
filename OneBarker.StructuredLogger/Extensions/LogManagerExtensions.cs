using System.Text.RegularExpressions;
using OneBarker.StructuredLogger.Internal;

namespace OneBarker.StructuredLogger.Extensions;

public static class LogManagerExtensions
{
    internal static string CreateSafeName(this ILogManager _, string value, string defaultValue)
    {
        var ret = Regex.Replace(Regex.Replace(value, @"\s+", "_"), @"[^A-Za-z0-9._-]", "").Trim('.', '-', '_');

        return string.IsNullOrEmpty(ret) ? defaultValue : ret;
    }

    internal static string CheckDirectory(this ILogManager _, string logFileDirectory)
    {
        logFileDirectory = logFileDirectory.Replace('\\', '/').TrimEnd('/');
        if (string.IsNullOrWhiteSpace(logFileDirectory)) throw new ArgumentException("LogFileDirectory cannot be blank.");
        if (!Directory.Exists(logFileDirectory)) Directory.CreateDirectory(logFileDirectory);
        var test = $"Test data written at {DateTime.Now:HH:mm:ss} on {DateTime.Today:MM/dd/yyyy}.\r\nThis file is safe to delete.\r\n";
        var file = logFileDirectory + "/test.txt";
        File.WriteAllText(file, test);
        File.Delete(file);
        return logFileDirectory;
    }

    internal static int DateToInt(this ILogManager _, DateTime date)
        => (date.Year * 10000) + (date.Month * 100) + (date.Day);

    internal static long TimeToLong(this ILogManager _, DateTime time, int extra)
    {
        var ticks = time.Ticks;
        ticks -= ticks % 10000;
        ticks += extra;
        return ticks;
    }
    
    internal static IEnumerable<ILogEntry> ReadFile(this ILogManager _, string filename)
    {
        using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var reader = new StreamReader(stream))
        {
            while (reader.ReadLine() is { } line)
            {
                yield return LogEntry.FromJson(line, false);
            }
        }
    }
}
