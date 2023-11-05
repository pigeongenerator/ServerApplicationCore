using ApplicationCore.Data;
using System;

namespace ApplicationCore;

internal class EventManager
{
    private static EventManager? _instance;
    private AppState _state = AppState.Stopped;

    public event Action? StartEvent;
    public event Action? StopEvent;

    internal static EventManager Instance {
        get {
            _instance ??= new EventManager(); //if instance is null, a new instance is created
            return _instance; //returns the instance
        }
    }

    /// <summary>
    /// holds the current state of the application
    public AppState State {
        get => _state;
    }

    /// <summary>
    /// calls all members of '<see cref="StartEvent"/>' if '<see cref="State"/>' is '<see cref="AppState.Stopped"/>'
    /// </summary>
    /// <exception cref="InvalidOperationException">thrown when '<see cref="State"/>' is not equal to '<see cref="AppState.Stopped"/>'</exception>
    public void Start()
    {
        //if the application state is not stopped, block the method call
        if (_state != AppState.Stopped) {
            throw new InvalidOperationException($"Can't start the application in it's current state: '{_state}'");
        }

        //set the application's state to starting
        _state = AppState.Starting;

        //invoke the start event
        StartEvent?.Invoke();

        //start event finished calling, set the application's state to running
        _state = AppState.Running;
    }

    /// <summary>
    /// calls all members of '<see cref="StopEvent"/>' if '<see cref="State"/>' is '<see cref="AppState.Running"/>'
    /// </summary>
    /// <exception cref="InvalidOperationException">thrown when '<see cref="State"/>' is not equal to '<see cref="AppState.Running"/>'</exception>
    public void Stop()
    {
        //if the application state is not running, block the method call
        if (_state != AppState.Running) {
            throw new InvalidOperationException($"Can't start the application in it's current state: '{_state}'");
        }

        //set the application's state to stopping
        _state = AppState.Stopping;

        //invoke the stop event
        StopEvent?.Invoke();

        //stop event finished calling, set the application's state to stopped
        _state = AppState.Stopped;
    }
}
