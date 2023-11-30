using ChaosMode.API;

namespace ChaosMode.BattleBitCommands;

public class ToggleVoteCommand: InGameCommand
{
    public ToggleVoteCommand(BattleBitServer r) : base(r)
    {
        CommandString = "toggleVote";
        Description = "Toggles Votes for yourself";
        Permission = 20;
        NeedsUserInput = false;
    }

    public override void CommandCallback(BattleBitPlayer commandSource)
    {
        R.BroadcasterList[commandSource.SteamID].AcceptsVotes = !R.BroadcasterList[commandSource.SteamID].AcceptsVotes;
        commandSource.SayToChat($"Votes are now {R.BroadcasterList[commandSource.SteamID].AcceptsVotes}");

    }

    public override void CommandCallback(BattleBitPlayer commandSource, string commandValue)
    {
        throw new System.NotImplementedException();
    }
}