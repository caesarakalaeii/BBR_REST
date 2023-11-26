using ChaosMode.API;

namespace ChaosMode.BattleBitCommands;

public class AddPermissionCommand: InGameCommand
{
    public AddPermissionCommand(BattleBitServer r) : base(r)
    {
        CommandString = "addPerm";
        Description = "Adds User with Permsission";
        Permission = 50;
        NeedsUserInput = true;
    }

    public override void CommandCallback(BattleBitPlayer commandSource)
    {
        throw new System.NotImplementedException();
    }

    public override void CommandCallback(BattleBitPlayer commandSource, string commandValue)
    {
        string[] parts = commandValue.Split(" ");
        ulong steamId = ulong.Parse(parts[0]);
        int permission = int.Parse(parts[1]);
        R.Permissions[steamId] = permission;
        R.SayToChat($"User with SteamId {steamId} has been granted {permission} Permission", commandSource);
        Program.Logger.Info($"User with SteamId {steamId} has been granted {permission} Permission by {commandSource.Name}({commandSource.SteamID})");
        
    }
}