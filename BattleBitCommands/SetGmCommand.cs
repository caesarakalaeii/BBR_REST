using System;
using System.Linq;
using ChaosMode.API;

namespace ChaosMode.BattleBitCommands;

public class SetGmCommand: InGameCommand
{
    
    public SetGmCommand(BattleBitServer r) : base(r)
    {
     CommandString = "setGM";
     Description = "Selects the next Gamemode in Playlist and resets all Players";
     Permission = 25;
     NeedsUserInput = true;


    }

    public override void CommandCallback(BattleBitPlayer commandSource)
    {
        throw new System.NotImplementedException();
    }

    public override void CommandCallback(BattleBitPlayer commandSource, string gamemodeName)
    {
        try
        {
            R.CurrentGameMode.Reset();
        }
        catch (Exception ex)
        {
            Program.Logger.Info($"ERROR resetting GM: {ex}");
        }

        foreach (var gameMode in R.GameModes.Where(gameMode => gameMode.Name == gamemodeName))
        {
            R.CurrentGameMode = gameMode;
            R.GameModeIndex = R.GameModes.IndexOf(gameMode);
        }

        try
        {
            R.CurrentGameMode.Init();
        }
        catch (Exception ex)
        {
            Program.Logger.Info($"ERROR initializing GM: {ex}");
        }

        R.AnnounceShort($"GameMode is now {R.CurrentGameMode.Name}");
    }
}