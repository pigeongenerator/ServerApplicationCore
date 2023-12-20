using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationCore;
public class ApplicationManager : IDisposable {
    private const string APPLICATION_DIRECTORY = "./Applications";
    private const int EXIT_DELAY = 1000;
    private static ApplicationManager? _instance;
    private readonly LogManager _logManager;
    private readonly Logger _log;
    private readonly List<Application> _applications;
    private readonly ManualResetEvent _shutdownEvent;

    //private constructor to insure there is only one 
    private ApplicationManager() {
        _logManager = new LogManager();
        _log = _logManager.CreateLogger("System");
        _applications = new List<Application>();
        _shutdownEvent = new ManualResetEvent(false);
        AppDomain.CurrentDomain.UnhandledException += ErrorHandler;
    }

    #region properties
    public static ApplicationManager Instance {
        get {
            _instance = new ApplicationManager();
            return _instance;
        }
    }

    public Logger Log {
        get => _log;
    }

    public LogManager LogManager {
        get => _logManager;
    }
    #endregion //properties

    #region application starting
    #region startup / shutdown
    public void Run() {
        Initialize();
        Start();
        _shutdownEvent.WaitOne();
        Log.WriteInfo("The application shut down successfully!");
        Thread.Sleep(EXIT_DELAY); //delay exit, to make sure all logs have been written
    }

    public void Stop(bool exitProgram = true) {
        List<Task> shutdownTask = new();
        Log.WriteInfo("stopping apps...");
        for (int i = 0; i < _applications.Count; i++) {
            Log.WriteInfo($"{(float)i / _applications.Count * 100}% stopping app '{_applications[i].GetType().FullName}'...");
            shutdownTask.Add(_applications[i].Stop());
        }

        Task.WhenAll(shutdownTask);
        Dispose();

        Log.WriteInfo("done!");

        if (exitProgram) {
            _shutdownEvent.Set();
        }
    }

    private void Start() {
        Log.WriteInfo("starting apps...");
        for (int i = 0; i < _applications.Count; i++) {
            Log.WriteInfo($"{(float)i / _applications.Count * 100}% starting app '{_applications[i].GetType().FullName}'...");
            _applications[i].Main();
        }

        Log.WriteInfo("done!");
    }
    #endregion //startup / shutdown

    private void Initialize() {
        string directoryPath = Path.GetFullPath(APPLICATION_DIRECTORY);
        Directory.CreateDirectory(directoryPath);
        string[] applicationPaths = Directory.GetFiles(directoryPath, "*.dll");
        foreach (string path in applicationPaths) {
            Assembly assembly = Assembly.LoadFile(path);
            foreach (Type type in assembly.GetTypes().Where(type => typeof(Application).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)) {
                object? obj = Activator.CreateInstance(type);
                if (obj == null) {
                    continue;
                }

                Application app = (Application)obj;
                _applications.Add(app);
                Log.WriteInfo($"loaded application: '{type.FullName}' ({assembly.GetName().Version})");
            }
        }
    }
    #endregion //application starting

    public void DisposeApplication(Application application) {
        if (application.Disposed == false) {
            application.Dispose();
            return;
        }

        _applications.Remove(application);
    }

    #region application finding
    public Application? FindApplicationOfType<T>() where T : Application, new() {
        return FindApplicationsOfType<T>().FirstOrDefault();
    }
    public IEnumerable<Application> FindApplicationsOfType<T>() where T : Application, new() {
        return
            from application in _applications
            where application is T
            select application as T;
    }
    #endregion //application finding

    //handles unhandled exceptions
    private void ErrorHandler(object sender, UnhandledExceptionEventArgs exception) {
        _log.WriteFatal($"There was an unhandled exception! {exception.ExceptionObject}");
        Thread.Sleep(EXIT_DELAY); //delay exit, to make sure all logs have been written
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
        while (_applications.Count > 0) {
            _applications[0].Dispose();
            _applications.RemoveAt(0);
        }
    }
}
