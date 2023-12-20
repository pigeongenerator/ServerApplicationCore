using ApplicationCore;
using System.Threading.Tasks;

namespace ApplicationTemplate;
public class ApplicationTemplateApp : Application {
    //called when the application starts
    public async override Task Main() {
        await Task.Delay(-1);
    }
    
    //called when the application stops
    public override Task Stop() {
        throw new System.NotImplementedException();
    }
}
