using System.Threading.Tasks;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using ChaosMode.API;

namespace ChaosMode.Modules;

public class IllegalPlayerActions : ServerModule
{
    /// <summary>
    /// Stolen from https://github.com/DasIschBims/Lifesteal
    /// </summary>
    
    public override Task<bool> OnPlayerJoinedSquad(BattleBitPlayer player, Squad<BattleBitPlayer> squad)
    {
        player.KickFromSquad();
        return Task.FromResult(false);
    }

    public override Task<bool> OnPlayerRequestingToChangeRole(BattleBitPlayer player, GameRole requestedRole)
    {
        if (requestedRole != GameRole.Assault)
            player.SetNewRole(GameRole.Assault);

        return Task.FromResult(false);
    }

    public IllegalPlayerActions(BattleBitServer server) : base(server)
    {
    }
}