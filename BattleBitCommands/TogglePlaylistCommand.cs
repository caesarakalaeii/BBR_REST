using ChaosMode.API;

namespace ChaosMode.BattleBitCommands;

public class TogglePlaylistCommand: InGameCommand
{
    public TogglePlaylistCommand(BattleBitServer r) : base(r)
    {
        CommandString = "togglePlaylist";
        Description = "Toggles the GamemodePlaylist";
        Permission = 25;
        NeedsUserInput = false;

    }

    public override void CommandCallback(BattleBitPlayer commandSource)
    {
        R.CyclePlaylist = !R.CyclePlaylist;
        
        R.AnnounceShort($"Playlist is now {R.CyclePlaylist}");
        Program.Logger.Info($"Playlist is now {R.CyclePlaylist}");
    }

    public override void CommandCallback(BattleBitPlayer commandSource, string commandValue)
    {
        throw new System.NotImplementedException();
    }
}