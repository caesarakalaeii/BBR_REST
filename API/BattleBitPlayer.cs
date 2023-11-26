using System.Linq;
using BattleBitAPI;
using ChaosMode.Enums;

namespace ChaosMode.API;

public class BattleBitPlayer : Player<BattleBitPlayer>
{
    public bool IsBroadcaster { get; set; } = false;
    
    public int Permission { get; set; }
    
    
    public PlayerRoles[] PlayerRoles = {
        ChaosMode.Enums.PlayerRoles.Default
    };

    public bool AddPlayerRole(PlayerRoles playerRole)
    {
        if (PlayerRoles.Contains(playerRole)) return false;
        
        var updatedRoles = new PlayerRoles[PlayerRoles.Length + 1];

        for (var i = 0; i < PlayerRoles.Length; i++)
        {
            updatedRoles[i] = PlayerRoles[i];
        }

        updatedRoles[PlayerRoles.Length] = playerRole;
        PlayerRoles = updatedRoles;

        return true;
    }

    public PlayerRoles GetHighestRole()
    {
        return PlayerRoles.Length == 0 ? ChaosMode.Enums.PlayerRoles.Default : PlayerRoles.Max();
    }

    public string GetPrefixForHighestRole(PlayerRoles highestRole)
    {
        return Program.ServerConfiguration.PlayerChatPrefixes.TryGetValue(highestRole.ToString(), out var prefix) ? prefix : Program.BattleBitServerRoles.Default;
    }
    
    public string GetSuffixForHighestRole(PlayerRoles highestRole)
    {
        return Program.ServerConfiguration.PlayerChatSuffixes.TryGetValue(highestRole.ToString(), out var prefix) ? prefix : Program.BattleBitServerRoles.Default;
    }

    


}