using ChaosMode.API;

namespace ChaosMode.BattleBitCommands;

public class GetGmCommand: InGameCommand
{
    public GetGmCommand(BattleBitServer r) : base(r)
    {
        CommandString = "getGM";
        Description = "Announces the current selected GameMode";
        Permission = 0;
    }

    public override void CommandCallback(BattleBitPlayer commandSource)
    {
        R.SayToChat($"GameMode is {R.CurrentGameMode.Name}", commandSource);
    }

    public override void CommandCallback(BattleBitPlayer commandSource, string commandValue)
    {
        throw new System.NotImplementedException();
    }
}