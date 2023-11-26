using System.Collections.Generic;

namespace ChaosMode.Commands;

public class CommandList
{
    public static List<ConsoleCommand> Commands { get; } = new()
    {
        new Help(),
        new Kick(),
        new PlayerList(),
        new BroadcasterList()
    };
}