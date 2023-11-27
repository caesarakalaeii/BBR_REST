using BattleBitAPI.Common;

namespace ChaosMode.API;

public class Swap : GameMode
{
    public Swap(BattleBitServer r) : base(r)
    {
        Name = "Swappers";
    }

    public override Returner OnPlayerSpawning(BattleBitPlayer player, OnPlayerSpawnArguments request)
    {
        player.Modifications.RunningSpeedMultiplier = 1.25f;
        player.Modifications.FallDamageMultiplier = 0f;
        player.Modifications.JumpHeightMultiplier = 1.5f;
        return base.OnPlayerSpawning(player, request);
    }

    public override OnPlayerKillArguments<BattleBitPlayer> OnAPlayerDownedAnotherPlayer(
        OnPlayerKillArguments<BattleBitPlayer> onPlayerKillArguments)
    {

        var victimPos = onPlayerKillArguments.VictimPosition;
        onPlayerKillArguments.Killer.Teleport(victimPos);
        onPlayerKillArguments.Victim.Kill();
        return base.OnAPlayerDownedAnotherPlayer(onPlayerKillArguments);
    }
}