using ApplicationCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationCore.Bases;

/// <summary>
/// defines an application module which runs on the server application
/// </summary>
public abstract class Application : LoggableObject
{
    private AppState _state;
    private readonly Action _startAction;
    private readonly Action _stopAction;

    //called when the class is constructed
    protected Application(string? logName = null) : base(logName)
    {
        _state = AppState.Stopped;

        ApplicationManager.Applications.Add(this); //add the application to the applications

        //create start and stop actions
        _startAction = () => ApplicationThreadManager.RunOnThread(Main, AppState.Running, this);
        _stopAction = () => ApplicationThreadManager.RunOnThread(Stop, AppState.Stopped, this);

        //add the methods as event listeners
        EventManager eventManager = EventManager.Instance;
        eventManager.StartEvent += _startAction;
        eventManager.StopEvent += _stopAction;
    }

    //is called when the class is deconstructed
    ~Application()
    {
        Dispose(); //just call dispose on finalise
    }

    public override void Dispose()
    {
        //TODO: add logic so dispose isn't called twice
        GC.SuppressFinalize(this); //suppress the finalise call of garbage collection because this already got it covered
        ApplicationManager.Applications.Remove(this); //remove this from the application list

        //remove the methods as event listeners
        EventManager eventManager = EventManager.Instance;
        eventManager.StartEvent -= _startAction;
        eventManager.StopEvent -= _stopAction;

        //dispose of the logger
        base.Dispose();
    }

    /// <summary>
    /// short-hand for <see cref="ApplicationManager.Instance"/>
    /// </summary>
    protected static ApplicationManager ApplicationManager {
        get => ApplicationManager.Instance;
    }

    /// <summary>
    /// holds the state of the application
    /// </summary>
    public AppState State {
        get => _state;
        internal set => _state = value;
    }

    public static T? FindApplicationOfType<T>() where T : Application
    {
        return (
            from application in ApplicationManager.Applications
            where application is T
            select application as T).FirstOrDefault();
    }

    public static IEnumerable<T> FindApplicationsOfType<T>() where T : Application
    {
        return
            from application in ApplicationManager.Applications
            where application is T
            select application as T;
    }

    /// <summary>
    /// called when the application starts
    /// </summary>
    protected abstract void Main();

    /// <summary>
    /// called when the application stops
    /// </summary>
    protected abstract void Stop();
}
