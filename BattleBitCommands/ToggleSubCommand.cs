using ChaosMode.API;

namespace ChaosMode.BattleBitCommands;

public class ToggleSubCommand: InGameCommand

{
    public ToggleSubCommand(BattleBitServer r) : base(r)
    {
        CommandString = "toggleSub";
        Description = "Toggles accepting Subs and Gifts for yourself";
        Permission = 20;
        NeedsUserInput = false;
    }

    public override void CommandCallback(BattleBitPlayer commandSource)
    {
        R.BroadcasterList[commandSource.SteamID].AcceptsSubs = !R.BroadcasterList[commandSource.SteamID].AcceptsSubs;
        commandSource.SayToChat($"AcceptsSubs is now {R.BroadcasterList[commandSource.SteamID].AcceptsSubs}");
    }

    public override void CommandCallback(BattleBitPlayer commandSource, string commandValue)
    {
        throw new System.NotImplementedException();
    }
}