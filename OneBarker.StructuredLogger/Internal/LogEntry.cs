using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace OneBarker.StructuredLogger.Internal;

internal sealed class LogEntry : ILogEntry
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder                = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        AllowTrailingCommas    = true,
        WriteIndented          = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private const string DeserializeError = "Could not deserialize LogEntry from provided JSON.";

    internal static string ToJson(ILogEntry entry)
        => Cast(entry).ToJson();

    internal static LogEntry Cast(ILogEntry entry)
    {
        // make sure we only serialize the entry according to the interface.
        if (entry is not LogEntry le)
        {
            le = new LogEntry()
            {
                LogTime       = entry.LogTime,
                LogLevel      = entry.LogLevel,
                LogEvent      = entry.LogEvent,
                LogText       = entry.LogText,
                LogException  = entry.LogException,
                LogParameters = entry.LogParameters
            };
        }

        return le;
    }
    
    public DateTime                                     LogTime       { get; set; } = DateTime.MinValue;
    public LogLevel                                     LogLevel      { get; set; } = LogLevel.None;
    public int                                          LogEvent      { get; set; } = 0;
    public string                                       LogText       { get; set; } = "";
    public string?                                      LogException  { get; set; } = null;
    public IReadOnlyList<KeyValuePair<string, object?>> LogParameters { get; set; } = Array.Empty<KeyValuePair<string, object?>>();

    internal string ToJson()
        => JsonSerializer.Serialize(this, JsonOptions);

    internal static LogEntry FromJson(string json, bool throwOnError = true)
    {
        try
        {
            return JsonSerializer.Deserialize<LogEntry>(json, JsonOptions)
                   ?? throw new JsonException("");
        }
        catch (JsonException jex)
        {
            var isNull  = string.IsNullOrWhiteSpace(jex.Message);
            
            if (throwOnError)
            {
                if (!isNull) throw; // rethrow the original.
                throw new JsonException(DeserializeError);
            }

            return new LogEntry()
            {
                LogTime  = DateTime.Now,
                LogLevel = LogLevel.Error,
                LogText  = isNull ? DeserializeError : jex.Message,
                LogParameters = new[]
                {
                    new KeyValuePair<string, object?>("JSON", json)
                }
            };
        }
    }
}

