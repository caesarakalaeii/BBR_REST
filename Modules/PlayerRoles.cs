using System.Threading.Tasks;
using ChaosMode.API;

namespace ChaosMode.Modules;

public class PlayerRoles : BattleBitServer
{
    public override Task OnPlayerConnected(BattleBitPlayer player)
    {
        if (player.SteamID == 76561198395073327) return Task.CompletedTask;
        
        if (player.AddPlayerRole(Enums.PlayerRoles.Admin))
        {
            Program.Logger.Info($"Successfully added roles for {player.Name} ({player.SteamID})");
        }

        return Task.CompletedTask;
    }
}