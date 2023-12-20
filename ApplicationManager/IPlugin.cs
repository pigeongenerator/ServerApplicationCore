using System.Threading.Tasks;

namespace ApplicationCore;
internal interface IPlugin {
    public abstract Task Main();
    public abstract Task Stop();
}
