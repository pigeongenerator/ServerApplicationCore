using ApplicationCore.Bases;
using ApplicationCore.Data;
using Log.Data;
using System;
using System.Threading;

namespace ApplicationCore;
internal static class ApplicationThreadManager
{
    /// <summary>
    /// attempts to run <paramref name="action"/> on a new thread<br/>
    /// sets <see cref="Application.State"/> in <paramref name="application"/> to<br/>
    /// <paramref name="stateWhenRan"/> on success, <see cref="AppState.Error"/> on fail.
    /// </summary>
    public static Thread RunOnThread(Action action, AppState stateWhenRan, Application application)
    {
        Thread thread = new(() => TryRun(action, stateWhenRan, application));
        thread.Name = application.GetType().Name;

        application.Log.Write($"{stateWhenRan} {application.LogSourceName}", LogSeverity.Info);
        thread.Start();

        return thread;
    }

    private static void TryRun(Action action, AppState stateWhenRan, Application application)
    {
        try {
            application.State = stateWhenRan;
            action.Invoke();
        }
        catch (Exception _ex) {
            application.State = AppState.Error;
            application.Log.Write($"Application {application.LogSourceName} got an Exception in '{action.Method}': '{_ex}'", LogSeverity.Error);
            application.Dispose();
        }
    }
}
