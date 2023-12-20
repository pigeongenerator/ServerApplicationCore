using System.Threading.Tasks;

namespace ApplicationCore;
internal interface IPlugin {
    /// <summary>
    /// entry-point, is ran to start the application. Make sure to utilize async to not block the main thread
    /// </summary>
    public abstract Task Main();

    /// <summary>
    /// called when the application needs to quit
    /// </summary>
    public abstract Task Stop();
}
