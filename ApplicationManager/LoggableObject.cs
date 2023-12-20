using Log;
using Log.Data;
using System;

namespace ApplicationCore;
public abstract class LoggableObject : IDisposable {
    private readonly string _logSourceName;
    private readonly Logger _log;
    private bool _disposed;

    public LoggableObject(string? sourceName = null) {
        _logSourceName = sourceName ?? GetType().Name;
        _log = ApplicationManager.Instance.LogManager.CreateLogger(_logSourceName);
        _disposed = false;
    }

    ~LoggableObject() {
        if (_disposed == false) {
            Dispose();
        }
    }

    public string LogSourceName {
        get => _logSourceName;
    }

    public Logger Log {
        get => _log;
    }

    //short-hands for log methods
    public void Write(LogEntry entry) => _log.Write(entry);
    public void Write(string message, LogSeverity severity) => _log.Write(message, severity);
    public void WriteInfo(string message) => _log.WriteInfo(message);
    public void WriteWarning(string message) => _log.WriteWarning(message);
    public void WriteDebug(string message) => _log.WriteDebug(message);
    public void WriteError(string message) => _log.WriteError(message);
    public void WriteFatal(string message) => _log.WriteFatal(message);

    public virtual void Dispose() {
        if (_disposed == true) {
            throw new Exception($"{nameof(LogManager)} has already been disposed");
        }

        _disposed = true;
        GC.SuppressFinalize(this);
        ApplicationManager.Instance.LogManager.DestroyLogger(_logSourceName);
    }
}
