using System.Threading.Tasks;
using ChaosMode.API;

namespace ChaosMode.Modules;

public class LoadingScreenText : ServerModule
{
    /// <summary>
    /// Stolen from https://github.com/DasIschBims/Lifesteal
    /// </summary>
    
    public override Task OnConnected()
    {
        Server.LoadingScreenText = Program.ServerConfiguration.LoadingScreenText;
        
        return Task.CompletedTask;
    }

    public LoadingScreenText(BattleBitServer server) : base(server)
    {
    }
}