<<<<<<< Updated upstream
﻿using ApplicationCore.Bases;
using ApplicationCore.Data;
using Log;
using Log.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
=======
﻿using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
>>>>>>> Stashed changes
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationCore;
public class ApplicationManager : IDisposable {
    private const string APPLICATION_DIRECTORY = "./Applications";
    private static ApplicationManager? _instance;
    private readonly LogManager _logManager;
    private readonly Logger _log;
    private readonly List<Application> _applications;
    private readonly ManualResetEvent _shutdownEvent;

<<<<<<< Updated upstream
public class ApplicationManager
{
    private const int ExitDelay = 10;                       //the delay in milliseconds before the application exits
    //TODO: make version more easily editable, also make so the version is viewable in data.
    private readonly static Version _version = new(5, 0);   //the current version of the application
    private static ApplicationManager? _instance;           //the instance of the application manager

    private readonly string _
    private readonly ManualResetEvent _shutDownEvent;   //the thread until the application has shut down
    private readonly List<Application> _applications;   //the applications that are currently active
    private readonly EventManager _eventManager;        //manages startup and shutdown events within the application
    private readonly LogManager _logManager;            //for managing log entries being generated
    private readonly Logger _log;                       //the application core's logger

    //private constructor to insure there is only one 
    private ApplicationManager()
    {
        _shutDownEvent = new ManualResetEvent(false);
        _applications = new List<Application>();
        _eventManager = EventManager.Instance;
        _logManager = new LogManager();
        _log = _logManager.CreateLogger("Core");

        AppDomain.CurrentDomain.UnhandledException += ErrorHandler;
=======
    private ApplicationManager() {
        _logManager = new LogManager();
        _log = _logManager.CreateLogger("System");
        _applications = new List<Application>();
        _shutdownEvent = new ManualResetEvent(false);
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
    public AppState State {
        get => _eventManager.State;
    }

    internal List<Application> Applications {
        get => _applications;
    }
    #endregion

    #region methods
    #region startup/shutdown
    /// <summary>
    /// runs the application
    /// </summary>
    public void RunApplication()
    {
        _log.Write($"Starting Application v{_version}", LogSeverity.Info);
        _eventManager.Start();

        AwaitExit();
    }

    /// <summary>
    /// shuts down the application
    /// </summary>
    public void Shutdown()
    {
        _log.Write("Shutting Down", LogSeverity.Info);
        _eventManager.Stop();

        //write an error if an application failed to shut down
        foreach (Application application in _applications) {
            if (application.State == AppState.Stopped) {
                _log.WriteError($"something went wrong when shutting down the application '{application.GetType()}', the application didn't shut down!");
            }
        }

        //communicate that the application has shut down
        _shutDownEvent.Set();
=======
    #region application starting
    #region startup / shutdown
    public void Run() {
        Initialize();
        Start();
        _shutdownEvent.WaitOne();
    }

    public void Stop() {
        List<Task> shutdownTask = new();
        for (int i = 0; i < _applications.Count; i++) {
            Log.WriteInfo($"{(float)i / _applications.Count * 100}% stopping app '{_applications[i].GetType().FullName}'...");
            shutdownTask.Add(_applications[i].Stop());
        }

        Task.WhenAll(shutdownTask);
        Log.WriteInfo("done!");
        _shutdownEvent.Set();
    }

    private void Start() {
        for (int i = 0; i < _applications.Count; i++) {
            Log.WriteInfo($"{(float)i / _applications.Count * 100}% starting app '{_applications[i].GetType().FullName}'...");
            _applications[i].Main();
        }

        Log.WriteInfo("done!");
    }
    #endregion //startup / shutdown

    private void Initialize() {
        string[] applicationPaths = Directory.GetFiles(APPLICATION_DIRECTORY, "*.dll");
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
>>>>>>> Stashed changes
    }
    #endregion

<<<<<<< Updated upstream
    //holds the logic for when the application exists
    private void AwaitExit()
    {
        _shutDownEvent.WaitOne(); //wait till the shutdownEvent is called
        _log.WriteInfo("The application safely shut down");
        Task.Delay(ExitDelay).Wait(); //delay exit, to make sure all logs have been written
=======
    #region application finding
    public Application? FindApplicationOfType<T>() where T : Application, new() {
        return FindApplicationsOfType<T>().FirstOrDefault();
>>>>>>> Stashed changes
    }
    public IEnumerable<Application> FindApplicationsOfType<T>() where T : Application, new() {
        return
            from application in _applications
            where application is T
            select application as T;
    }
    #endregion //application finding

<<<<<<< Updated upstream
    private void InitApplications()
    {
    }

    #region error handling
    //handles unhandled exceptions
    private void ErrorHandler(object sender, UnhandledExceptionEventArgs exception)
    {
        _log.WriteFatal($"There was an unhandled exception! {exception.ExceptionObject}");
        Task.Delay(ExitDelay).Wait(); //delay exit, to make sure all logs have been written
=======
    public void Dispose() {
        GC.SuppressFinalize(this);
        while (_applications.Count > 0) {
            _applications[0].Dispose();
            _applications.RemoveAt(0);
        }
>>>>>>> Stashed changes
    }
}
