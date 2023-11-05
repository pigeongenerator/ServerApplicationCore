namespace ApplicationCore.Data;

public enum AppState : byte
{
    /// <summary>
    /// currently shut down
    /// </summary>
    Stopped,

    /// <summary>
    /// in the process of starting
    /// </summary>
    Starting,

    /// <summary>
    /// currently running
    /// </summary>
    Running,

    /// <summary>
    /// in the process of stopping
    /// </summary>
    Stopping,

    /// <summary>
    /// something went wrong and the application stopped
    /// </summary>
    Error,
}