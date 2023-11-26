using ChaosMode.API;

namespace ChaosMode.BattleBitCommands;

public class ForceStartCommand: InGameCommand
{
    
    public ForceStartCommand(BattleBitServer r) : base(r)
    {
        CommandString = "start";

        Description = "Starts the Game, disregarding player count";

        Permission = 25;
        
        NeedsUserInput = false;

    }

    public override void CommandCallback(BattleBitPlayer commandSource)
    {
        R.ForceStartGame();

        R.AnnounceShort($"Starting Game");
        Program.Logger.Info($"Starting Game");
    }

    public override void CommandCallback(BattleBitPlayer commandSource, string commandValue)
    {
        throw new System.NotImplementedException();
    }
}