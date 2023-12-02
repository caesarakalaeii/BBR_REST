using ChaosMode.API;

namespace ChaosMode.BattleBitCommands;

public class ToggleCheerCommand: InGameCommand
{
    public ToggleCheerCommand(BattleBitServer r) : base(r)
    {
        CommandString = "toggleCheer";
        Description = "Toggles accepting Cheers(Bits) for yourself";
        Permission = 20;
        NeedsUserInput = false;
    }

    public override void CommandCallback(BattleBitPlayer commandSource)
    {
        R.BroadcasterList[commandSource.SteamID].AcceptsCheers = !R.BroadcasterList[commandSource.SteamID].AcceptsCheers;
        commandSource.SayToChat($"AcceptsCheers is now {R.BroadcasterList[commandSource.SteamID].AcceptsCheers}");
    }

    public override void CommandCallback(BattleBitPlayer commandSource, string commandValue)
    {
        throw new System.NotImplementedException();
    }
}