using BattleBitAPI.Common;

namespace ChaosMode.API;

public class LifeSteal : GameMode
{
    public LifeSteal(BattleBitServer r) : base(r)
    {
        Name = "LifeSteal";
    }

    public override OnPlayerKillArguments<BattleBitPlayer> OnAPlayerDownedAnotherPlayer(
        OnPlayerKillArguments<BattleBitPlayer> args)
    {
        args.Killer.SetHP(100);
        args.Victim.Kill();
        return base.OnAPlayerDownedAnotherPlayer(args);
    }


    public override BattleBitPlayer OnPlayerSpawned(BattleBitPlayer player)
    {
        player.Modifications.RunningSpeedMultiplier = 1.25f;
        player.Modifications.FallDamageMultiplier = 0f;
        player.Modifications.JumpHeightMultiplier = 1.5f;
        player.Modifications.DisableBleeding();
        return base.OnPlayerSpawned(player);
    }

}