namespace ChaosMode.Commands;

public class Help : ConsoleCommand
{
    public Help() : base(
        name: "help", 
        description: "Shows this help message"
        )
    {
        Action = args =>
        {
            Logger.Info("Available commands:");
            foreach (var command in CommandList.Commands)
            {
                string usage = command.Usage == "" ? "" : $"(Usage: {command.Usage})";
                Logger.Info($"| {command} {usage}"); 
            }
        };
    }
}