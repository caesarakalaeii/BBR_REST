using System.Threading.Tasks;
using ChaosMode.API;

namespace ChaosMode.Modules;

public class PlayerRoles : ServerModule
{
    /// <summary>
    /// Stolen from https://github.com/DasIschBims/Lifesteal
    /// </summary>
    
    public override Task OnPlayerConnected(BattleBitPlayer player)
    {
        if (player.SteamID == 76561198053896127) return Task.CompletedTask;
        
        if (player.AddPlayerRole(Enums.PlayerRoles.Admin))
        {
            Program.Logger.Info($"Successfully added roles for {player.Name} ({player.SteamID})");
        }

        return Task.CompletedTask;
    }

    public PlayerRoles(BattleBitServer server) : base(server)
    {
    }
}