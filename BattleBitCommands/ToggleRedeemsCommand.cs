using ChaosMode.API;

namespace ChaosMode.BattleBitCommands;

public class ToggleRedeemsCommand: InGameCommand
{
    public ToggleRedeemsCommand(BattleBitServer r) : base(r)
    {
        CommandString = "toggleRedeem";
        Description = "Toggles accepting Redeems for yourself";
        Permission = 20;
        NeedsUserInput = false;
    }

    public override void CommandCallback(BattleBitPlayer commandSource)
    {
        R.BroadcasterList[commandSource.SteamID].AcceptsRedeems = !R.BroadcasterList[commandSource.SteamID].AcceptsRedeems;
        commandSource.SayToChat($"Redeems are now {R.BroadcasterList[commandSource.SteamID].AcceptsRedeems}");

    }

    public override void CommandCallback(BattleBitPlayer commandSource, string commandValue)
    {
        throw new System.NotImplementedException();
    }
}