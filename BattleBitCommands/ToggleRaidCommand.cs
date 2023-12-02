using ChaosMode.API;

namespace ChaosMode.BattleBitCommands;

public class ToggleRaidCommand: InGameCommand
{
    public ToggleRaidCommand(BattleBitServer r) : base(r)
    {
        CommandString = "toggleRaid";
        Description = "Toggles accepting Raids for yourself";
        Permission = 20;
        NeedsUserInput = false;
    }

    public override void CommandCallback(BattleBitPlayer commandSource)
    {
        R.BroadcasterList[commandSource.SteamID].AcceptsRaids = !R.BroadcasterList[commandSource.SteamID].AcceptsRaids;
        commandSource.SayToChat($"AcceptsRaids is now {R.BroadcasterList[commandSource.SteamID].AcceptsRaids}");
    }

    public override void CommandCallback(BattleBitPlayer commandSource, string commandValue)
    {
        throw new System.NotImplementedException();
    }
}