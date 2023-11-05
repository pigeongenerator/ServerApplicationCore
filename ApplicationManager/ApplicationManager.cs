using ApplicationCore.Bases;
using ApplicationCore.Data;
using Log;
using Log.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationCore;

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
    }

    #region properties
    public static ApplicationManager Instance {
        get {
            _instance ??= new ApplicationManager(); //if instance is null, a new instance is created
            return _instance; //returns the instance
        }
    }

    public ReadOnlyCollection<Application> ApplicationList {
        get => _applications.AsReadOnly();
    }

    public LogManager LogManager {
        get => _logManager;
    }

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
    }
    #endregion

    //holds the logic for when the application exists
    private void AwaitExit()
    {
        _shutDownEvent.WaitOne(); //wait till the shutdownEvent is called
        _log.WriteInfo("The application safely shut down");
        Task.Delay(ExitDelay).Wait(); //delay exit, to make sure all logs have been written
    }

    private void InitApplications()
    {
    }

    #region error handling
    //handles unhandled exceptions
    private void ErrorHandler(object sender, UnhandledExceptionEventArgs exception)
    {
        _log.WriteFatal($"There was an unhandled exception! {exception.ExceptionObject}");
        Task.Delay(ExitDelay).Wait(); //delay exit, to make sure all logs have been written
    }
    #endregion
    #endregion
}
