using ChaosMode.API.GameModes;

namespace ChaosMode.API;

public class Hardcore : GameMode
{
    public Hardcore(BattleBitServer r) : base(r)
    {
        Name = "Hardcore";
    }

    public override BattleBitPlayer OnPlayerSpawned(BattleBitPlayer player)
    {
        player.Modifications.HitMarkersEnabled = false;
        player.Modifications.RunningSpeedMultiplier = 1.25f;
        player.Modifications.FallDamageMultiplier = 2f;
        player.Modifications.GiveDamageMultiplier = 2f;
        player.SetHP(50);
        return base.OnPlayerSpawned(player);
    }
}