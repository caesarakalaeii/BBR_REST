using ChaosMode.API;

namespace ChaosMode.BattleBitCommands;

public abstract class InGameCommand
{
    public string CommandString;

    public string Description;

    public int Permission;

    public bool NeedsUserInput;

    public BattleBitServer R;

    public abstract void CommandCallback(BattleBitPlayer commandSource);
    public abstract void CommandCallback(BattleBitPlayer commandSource, string commandValue);

    protected InGameCommand(BattleBitServer r)
    {
        R = r;
    }
}