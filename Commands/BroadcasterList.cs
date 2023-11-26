namespace ChaosMode.Commands;

public class BroadcasterList: ConsoleCommand
{
    public BroadcasterList() : base
    (
        name: "broadcasters",
        description: "lists known broadcasters"
    )
    {
        Action = args =>
        {
            Logger.Info("Known broadcasters:");
            foreach (var caster in Server.BroadcasterList.Values)
            {
                Logger.Info($"Steam ID: {caster.SteamId}");
            }
        };
    }
}