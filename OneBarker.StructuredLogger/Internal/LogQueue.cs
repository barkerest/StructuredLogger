using System.Text;

namespace OneBarker.StructuredLogger.Internal;

internal class LogQueue<TManager> : IDisposable where TManager : class, ILogManager, IWriteData
{
    private readonly List<ILogEntry>          _queue;
    private readonly TManager                 _manager;
    private readonly Thread                   _thread;
    private          bool                     _quitThread;
    private          bool                     _flushing;

    public LogQueue(TManager manager)
    {
        _manager   = manager;
        _queue     = new List<ILogEntry>();
        _thread              = new Thread(Worker)
        {
            IsBackground = true
        };
        _quitThread          = false;
        _thread.Start();
    }

    public void Queue(ILogEntry entry)
    {
        lock (_queue)
        {
            _queue.Add(entry);
        }
    }

    public void Flush()
    {
        if (_thread.ThreadState is ThreadState.Aborted or ThreadState.Stopped) throw new InvalidOperationException("Thread is not running.");
        
        _flushing = true;
        while (_flushing) Thread.Sleep(1);
    }
    
    private void Worker()
    {
        while (true)
        {
            ILogEntry[] q;

            lock (_queue)
            {
                q = _queue.ToArray();
                _queue.Clear();
            }

            if (q.Any())
            {
                foreach (var entry in q)
                {
                    var record = LogEntry.ToJson(entry) + ",\n";
                    var data   = Encoding.UTF8.GetBytes(record);
                    _manager.WriteData(data);
                }
                _manager.Flush();
            }
            
            _flushing = false;
            
            if (_quitThread) return;
            Thread.Sleep(1);
        }
    }

    public void Dispose()
    {
        _quitThread = true;
        _thread.Join();
    }
}
