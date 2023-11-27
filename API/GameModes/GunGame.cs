using System.Collections.Generic;
using BattleBitAPI.Common;

namespace ChaosMode.API;

public class GunGame : GameMode
{
    private readonly GunGamePlayerData _data = new();

    private readonly List<Weapon> _mGunGame = new()
    {
        Weapons.Glock18,
        Weapons.Groza,
        Weapons.ACR,
        Weapons.AK15,
        Weapons.AK74,
        Weapons.G36C,
        Weapons.HoneyBadger,
        Weapons.KrissVector,
        Weapons.L86A1,
        Weapons.L96,
        Weapons.M4A1,
        Weapons.M9,
        Weapons.M110,
        Weapons.M249,
        Weapons.MK14EBR,
        Weapons.MK20,
        Weapons.MP7,
        Weapons.PP2000,
        Weapons.SCARH,
        Weapons.SSG69
    };

    public GunGame(BattleBitServer r) : base(r)
    {
        Name = "GunGame";
        GunGamePlayerData data = new();
    }

    public override void Init()
    {

        R.ServerSettings.TeamlessMode = true;
    }


    // Gun Game
    public override Returner OnPlayerSpawning(BattleBitPlayer player, OnPlayerSpawnArguments request)
    {
        UpdateWeapon(player);
        return base.OnPlayerSpawning(player, request);
    }

    public override BattleBitPlayer OnPlayerSpawned(BattleBitPlayer player)
    {
        player.Modifications.RespawnTime = 0f;
        player.Modifications.RunningSpeedMultiplier = 1.25f;
        player.Modifications.FallDamageMultiplier = 0f;
        player.Modifications.JumpHeightMultiplier = 1.5f;
        player.Modifications.DisableBleeding();
        return base.OnPlayerSpawned(player);
    }

    public int GetGameLenght()
    {
        return _mGunGame.Count;
    }

    public void UpdateWeapon(BattleBitPlayer player)
    {
        var w = new WeaponItem
        {
            ToolName = _mGunGame[_data.GetLevel(player)].Name,
            MainSight = Attachments.RedDot
        };


        player.SetPrimaryWeapon(w, 10, true);
    }

    public override OnPlayerKillArguments<BattleBitPlayer> OnAPlayerDownedAnotherPlayer(
        OnPlayerKillArguments<BattleBitPlayer> onPlayerKillArguments)
    {
        var killer = onPlayerKillArguments.Killer;
        var victim = onPlayerKillArguments.Victim;
        _data.IncLevel(killer);
        if (_data.GetLevel(killer) == GetGameLenght()) R.AnnounceShort($"{killer.Name} only needs 1 more Kill");
        if (_data.GetLevel(killer) > GetGameLenght())
        {
            R.AnnounceShort($"{killer.Name} won the Game");
            R.ForceEndGame();
        }

        killer.SetHP(100);
        victim.Kill();
        if (onPlayerKillArguments.KillerTool == "Sledge Hammer" && _data.GetLevel(victim) != 0) _data.DecLevel(victim);
        UpdateWeapon(killer);
        return base.OnAPlayerDownedAnotherPlayer(onPlayerKillArguments);
    }


    public override void Reset()
    {
        R.SayToAllChat("Resetting GameMode");
        R.ServerSettings.TeamlessMode = false;
        foreach (var player in R.AllPlayers)
        {
            _data.SetLevel(player, 0);
            player.Kill();
        }
    }
}