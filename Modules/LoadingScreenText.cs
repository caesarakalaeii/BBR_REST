using System.Threading.Tasks;
using ChaosMode.API;

namespace ChaosMode.Modules;

public class LoadingScreenText : BattleBitServer
{
    public override Task OnConnected()
    {
        LoadingScreenText = Program.ServerConfiguration.LoadingScreenText;
        
        return Task.CompletedTask;
    }
}