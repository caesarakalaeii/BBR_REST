using BattleBitAPI.Common;
using ChaosMode.API.GameModes;

namespace ChaosMode.API;

public class Csgo : GameMode
{
    public Csgo(BattleBitServer r) : base(r)
    {
        Name = "CSGO";
    }

    public override void Init()
    {
        R.ServerSettings.PlayerCollision = true;
        R.ServerSettings.FriendlyFireEnabled = true;
        if (R.Gamemode != "Rush")
        {
            // dunno
        }
    }
    //Buy system maybe


    public override OnPlayerKillArguments<BattleBitPlayer> OnAPlayerDownedAnotherPlayer(
        OnPlayerKillArguments<BattleBitPlayer> args)
    {
        var victim = args.Victim;
        var killer = args.Killer;
        victim.Modifications.CanDeploy = false;
        victim.Modifications.CanSpectate = false;
        victim.Kill();
        return base.OnAPlayerDownedAnotherPlayer(args);
    }

    public override void Reset()
    {
        R.ServerSettings.PlayerCollision = false;
        R.ServerSettings.FriendlyFireEnabled = false;
        base.Reset();
    }
}