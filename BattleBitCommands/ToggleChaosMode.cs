using ChaosMode.API;
using ChaosMode.Commands;

namespace ChaosMode.BattleBitCommands;

public class ToggleChaosMode : InGameCommand
{
    public ToggleChaosMode(BattleBitServer r) : base(r)
    {
        CommandString = "toggleChaos";
        Description = "Toggles ChaosMode for yourself";
        Permission = 20;
        NeedsUserInput = false;
    }

    public override void CommandCallback(BattleBitPlayer commandSource)
    {
        R.BroadcasterList[commandSource.SteamID].ChaosEnabled = !R.BroadcasterList[commandSource.SteamID].ChaosEnabled;
        
    }

    public override void CommandCallback(BattleBitPlayer commandSource, string commandValue)
    {
        throw new System.NotImplementedException();
    }
}