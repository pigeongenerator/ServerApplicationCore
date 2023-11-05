using Log;
using Log.Data;
using System;

namespace ApplicationCore.Bases;

/// <summary>
/// defines an object that can generate logs
/// </summary>
public abstract class LoggableObject : IDisposable
{
    private readonly string _logSourceName;
    private readonly Logger _log;

    public LoggableObject(string? sourceName = null)
    {
        _logSourceName = sourceName ?? GetType().Name;

        LogManager logManager = ApplicationManager.Instance.LogManager; //get the log manager
        _log = logManager.CreateLogger(_logSourceName); //create a logger for this application
    }

    //finalizer
    ~LoggableObject()
    {
        Dispose();
    }

    /// <summary>
    /// gets the name of what the source is of this loggable object
    /// </summary>
    public string LogSourceName {
        get => _logSourceName;
    }

    /// <summary>
    /// gets the logger
    /// </summary>
    public Logger Log {
        get => _log;
    }

    //short-hand for log methods
    public void Write(LogEntry entry) => _log.Write(entry);
    public void Write(string message, LogSeverity severity) => _log.Write(message, severity);
    public void WriteInfo(string message) => _log.WriteInfo(message);
    public void WriteWarning(string message) => _log.WriteWarning(message);
    public void WriteDebug(string message) => _log.WriteDebug(message);
    public void WriteError(string message) => _log.WriteError(message);
    public void WriteFatal(string message) => _log.WriteFatal(message);

    //TDOD: make sure dispose isn't called twice
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        _log.Dispose();
    }
}
