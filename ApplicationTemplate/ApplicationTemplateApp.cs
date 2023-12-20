using ApplicationCore;
using System.Threading.Tasks;

namespace ApplicationTemplate;
public class ApplicationTemplateApp : Application {
    //called when the application starts
    public async override Task Main() {
        await Task.Delay(-1); //use 'await' to give control back to the main thread
    }
    
    //called when the application stops
    public override Task Stop() {
        return Task.CompletedTask;
    }
}
