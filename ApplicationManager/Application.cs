using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationCore;
public abstract class Application : LoggableObject, IPlugin {
    private bool _disposed;

    protected internal Application() : base() {
        _disposed = false;
    }

    ~Application() {
        if (_disposed == false) {
            Dispose();
        }
    }

    public bool Disposed {
        get => _disposed;
    }

    public static ApplicationManager ApplicationManager {
        get => ApplicationManager.Instance;
    }

    #region application finding
    public static Application? FindApplicationOfType<T>() where T : Application, new() {
        return ApplicationManager.FindApplicationOfType<T>();
    }

    public static IEnumerable<Application> FindApplicationsOfType<T>() where T : Application, new() {
        return ApplicationManager.FindApplicationsOfType<T>();
    }
    #endregion //application finding

    public override void Dispose() {
        if (_disposed == true) {
            throw new Exception($"{nameof(Application)} has already been disposed");
        }

        _disposed = true;
        base.Dispose();
        GC.SuppressFinalize(this);

        ApplicationManager.Instance.DisposeApplication(this);
    }

    public abstract Task Main();
    public abstract Task Stop();
}
