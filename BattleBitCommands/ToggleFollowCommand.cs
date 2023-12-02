using ChaosMode.API;

namespace ChaosMode.BattleBitCommands;

public class ToggleFollowCommand: InGameCommand
{
    public ToggleFollowCommand(BattleBitServer r) : base(r)
    {
        CommandString = "toggleFollow";
        Description = "Toggles accepting Follow for yourself";
        Permission = 20;
        NeedsUserInput = false;
    }

    public override void CommandCallback(BattleBitPlayer commandSource)
    {
        R.BroadcasterList[commandSource.SteamID].AcceptsFollows = !R.BroadcasterList[commandSource.SteamID].AcceptsFollows;
        commandSource.SayToChat($"AcceptsFollows is now {R.BroadcasterList[commandSource.SteamID].AcceptsFollows}");
    }

    public override void CommandCallback(BattleBitPlayer commandSource, string commandValue)
    {
        throw new System.NotImplementedException();
    }
}