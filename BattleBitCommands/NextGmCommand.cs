using ChaosMode.API;

namespace ChaosMode.BattleBitCommands;

public class NextGmCommand: InGameCommand
{

    
    
    public NextGmCommand(BattleBitServer r) : base(r)
    {
        CommandString = "nextGM";
        Description = "Selects the next Gamemode in Playlist and resets all Players";
        Permission = 25;
        NeedsUserInput = false;

    }

    public override void CommandCallback(BattleBitPlayer commandSource)
    {
        R.CurrentGameMode.Reset();
        R.GameModeIndex = (R.GameModeIndex + 1) % R.GameModes.Count;
        R.CurrentGameMode = R.GameModes[R.GameModeIndex];
        R.CurrentGameMode.Init();
        
        R.AnnounceShort($"GameMode is now {R.CurrentGameMode.Name}");
        Program.Logger.Info($"GameMode is now {R.CurrentGameMode.Name}");
    }

    public override void CommandCallback(BattleBitPlayer commandSource, string commandValue)
    {
        throw new System.NotImplementedException();
    }
}