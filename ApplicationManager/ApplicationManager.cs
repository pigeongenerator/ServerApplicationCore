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
    private const string APPLICATION_DIRECTORY = "./Applications";  //the folder in which the applications will be
    private const int EXIT_DELAY = 1000;                            //the delay before the application exits
    private static ApplicationManager? _instance;       //holds the instance for the singleton
    private readonly LogManager _logManager;            //manages loggers
    private readonly Logger _log;                       //system's logger, for system messages
    private readonly List<Application> _applications;   //holds a list of the applications
    private readonly ManualResetEvent _shutdownEvent;   //prevents the thread from closing without exit signal

    //private constructor to insure there is only one 
    private ApplicationManager() {
        _logManager = new LogManager();
        _log = _logManager.CreateLogger("System");
        _applications = new List<Application>();
        _shutdownEvent = new ManualResetEvent(false);

        //add to the event listener
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
        Log.WriteInfo($"Running Server Application (v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(2)})");
        Initialize();
        Start();
        _shutdownEvent.WaitOne(); //wait till the shut down has been signaled
        Log.WriteInfo("The application shut down successfully!");
        Thread.Sleep(EXIT_DELAY); //delay exit, to make sure all logs have been written
    }

    public void Stop(bool exitProgram = true) {
        List<Task> shutdownTask = new();
        Log.WriteInfo("stopping apps...");

        //stop the apps
        for (int i = 0; i < _applications.Count; i++) {
            Log.WriteInfo($"{MathF.Round((float)i / _applications.Count * 100)}% stopping app '{_applications[i].GetType().FullName}'...");
            shutdownTask.Add(_applications[i].Stop());
        }
        
        //await when all have shut down, then dispose self
        Task.WhenAll(shutdownTask);
        Dispose();

        Log.WriteInfo("100% done!");

        if (exitProgram) {
            _shutdownEvent.Set();
        }
        else {
            Log.WriteWarning("The application shut down, but shutdown hasn't been signaled; the application will remain dormand and must be restarted externally.");
        }
    }

    private void Start() {
        Log.WriteInfo("starting apps...");

        //start the apps
        for (int i = 0; i < _applications.Count; i++) {
            Log.WriteInfo($"{MathF.Round((float)i / _applications.Count * 100)}% starting app '{_applications[i].GetType().FullName}'...");
            _applications[i].Main();
        }

        Log.WriteInfo("done!");
    }
    #endregion //startup / shutdown

    //adds all instances of the applications to the list
    private void Initialize() {
        //get '.dll' paths
        string directoryPath = Path.GetFullPath(APPLICATION_DIRECTORY); //correct the path
        Directory.CreateDirectory(directoryPath); //create a directory if it doesn't exist yet
        string[] applicationPaths = Directory.GetFiles(directoryPath, "*.dll"); //find the files that end with '.dll'

        //load the assemblies
        foreach (string path in applicationPaths) {
            Assembly assembly = Assembly.LoadFile(path);

            //load the applications
            foreach (Type type in assembly.GetTypes().Where(type => typeof(Application).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)) {
                //create an instance of the application
                object? obj = Activator.CreateInstance(type);

                if (obj == null) {
                    Log.WriteWarning($"failed to load: '{type.FullName}'");
                    continue;
                }

                //load the application
                Application app = (Application)obj;
                _applications.Add(app);
                Log.WriteInfo($"loaded application: '{type.FullName}' (v{assembly.GetName().Version?.ToString(2)})");
            }
        }
    }
    #endregion //application starting

    //disposes of an application
    public void DisposeApplication(Application application) {
        //if the call wasn't made from the application itself, call the application to dispose itself
        if (application.Disposed == false) {
            application.Dispose();
            return; //return, since the application calls back to this
        }

        //remove the application from the list
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

    //for logging unhandled exceptions
    private void ErrorHandler(object sender, UnhandledExceptionEventArgs exception) {
        _log.WriteFatal($"There was an unhandled exception! {exception.ExceptionObject}");
        Thread.Sleep(EXIT_DELAY); //delay exit, to make sure all logs have been written
    }

    public void Dispose() {
        GC.SuppressFinalize(this);

        //dispose of all applications (removes themselves from the list)
        while (_applications.Count > 0) {
            _applications[0].Dispose();
        }
    }
}
