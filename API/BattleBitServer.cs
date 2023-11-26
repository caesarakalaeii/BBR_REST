using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using ChaosMode.BattleBitCommands;

namespace ChaosMode.API;

public class BattleBitServer: GameServer<BattleBitPlayer>
{

    public Dictionary<ulong, Broadcaster> BroadcasterList = new();

    public Dictionary<ulong, int> Permissions = new();
    
    

    public void ConsumeCommand(RestEvent restEvent)
    {
        int? amount;
        string username;
        BattleBitPlayer? player;
        Program.Logger.Info($"Command recieved: {restEvent}");
        switch (restEvent.EventType)
        {
            case "Follow":
                
                break;
            case "Gift":
                
                break;
            case "Sub":
                
                break;
            case "SubBomb":
                
                break;
            case "Raid":
                
                break;
            case "Bits":
                amount = restEvent.Amount;
                
                break;
            case "Redeem":
                switch (restEvent.RedeemType)
                {
                    case "heal":
                        BroadcasterList[restEvent.SteamId].Player?.Heal(100);
                        break;
                    case "kill":
                        BroadcasterList[restEvent.SteamId].Player?.Kill();
                        break;
                        
                }

                break;
            case "AddBroadcaster":
                BroadcasterList.Add(restEvent.SteamId, new Broadcaster(restEvent.SteamId));
                player = AllPlayers.FirstOrDefault(p => p.SteamID == restEvent.SteamId);
                BroadcasterList[restEvent.SteamId].Player = player;
                break;
            case "RemoveBroadcaster":
                BroadcasterList.Remove(restEvent.SteamId);
                player = AllPlayers.FirstOrDefault(p => p.SteamID == restEvent.SteamId);
                if (player != null) player.IsBroadcaster = false;
                break;
            
                
        }

        
    }


    public readonly List<GameMode> GameModes;
    public readonly List<InGameCommand> InGameCommands;
    public GameMode CurrentGameMode;
    public bool CyclePlaylist;
    public int GameModeIndex;



    public BattleBitServer()
    {
        CyclePlaylist = false;
        GameModes = new List<GameMode>
        {
            new GunGame(this),
            new TeamGunGame(this),
            new LifeSteal(this),
            new Swap(this),
            new Hardcore(this),
            new MeleeOnly(this),
            new Csgo(this)
        };
        InGameCommands = new List<InGameCommand>
        {
            new ForceStartCommand(this),
            new GetGmCommand(this),
            new NextGmCommand(this),
            new SetGmCommand(this),
            new TogglePlaylistCommand(this),
            new AddPermissionCommand(this)
        };
        
        Permissions.Add(76561198053896127, 50); // Add Admin perms for caesar
        GameModeIndex = 0;
        CurrentGameMode = GameModes[GameModeIndex];
    }



    //GAMEMODE PASSTHROUGH

    public override Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitPlayer player, OnPlayerSpawnArguments request)
    {
        var ret = CurrentGameMode.OnPlayerSpawning(player, request);
        return base.OnPlayerSpawning(ret.Player, ret.SpawnArguments);
    }
    public override Task OnPlayerSpawned(BattleBitPlayer player)
    {
        var ret = CurrentGameMode.OnPlayerSpawned(player);
        return base.OnPlayerSpawned(ret);
    }

    public override Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<BattleBitPlayer> args)
    {
        
        var ret = CurrentGameMode.OnAPlayerDownedAnotherPlayer(args);
        return base.OnAPlayerDownedAnotherPlayer(ret);
    }

    public override Task OnPlayerGivenUp(BattleBitPlayer player)
    {
        var ret = CurrentGameMode.OnPlayerGivenUp(player);
        return base.OnPlayerGivenUp(ret);
    }
    
    public override Task OnPlayerConnected(BattleBitPlayer player)
    {
        if (BroadcasterList.Keys.Contains(player.SteamID))
        {
            BroadcasterList[player.SteamID].Player = player;
        }
        
        this.SayToAllChat("<color=green>" + player.Name + " joined the game!</color>");
        player.Message($"Current GameMode is: {CurrentGameMode.Name}", 4f);
        Program.Logger.Info("Connected: " + player);

        player.JoinSquad(Squads.Alpha);
        return Task.CompletedTask;
    }

    public override Task OnPlayerDisconnected(BattleBitPlayer player)
    {
        if (BroadcasterList.Keys.Contains(player.SteamID))
        {
            BroadcasterList[player.SteamID].Player = null;
        }
        Program.Logger.Info($"{player.Name} disconnected");
        this.SayToAllChat($"<color=red>{player.Name} left the game!</color>");
        CurrentGameMode.OnPlayerDisconnected(player);
        return Task.CompletedTask;
    }

    public override Task<bool> OnPlayerTypedMessage(BattleBitPlayer player, ChatChannel channel, string msg)
    {
        if (msg.StartsWith("!") && Permissions[player.SteamID] > 0)
        {
            string[] commandInput = msg.Split(" ");
            commandInput[0] = commandInput[0].Replace("!", "");
            foreach (var command in InGameCommands)
            {
                if (command.CommandString == commandInput[0] && Permissions[player.SteamID] >= command.Permission)
                {
                    try
                    {
                        
                        if (command.NeedsUserInput)
                        {
                            command.CommandCallback(player, msg.Replace($"!{command.CommandString} ", ""));
                        }
                        else
                        {
                            command.CommandCallback(player);
                        }
                    } catch (Exception ex){
                    {
                        SayToChat($"Could not invoke command {command.CommandString}, an Error occurred.", player);
                    }}
                }
                else if (command.CommandString == commandInput[0] && Permissions[player.SteamID] < command.Permission)
                {
                    Program.Logger.Info($"Command {command.CommandString} was invoked without sufficient Permissions by {player.Name}({player.SteamID})");
                    SayToChat("Not enough Permissions", player);
                }
            }
        }
        return base.OnPlayerTypedMessage(player, channel, msg);
    }
}

public class Returner
{
    public OnPlayerSpawnArguments SpawnArguments;
    public ChatChannel Channel;
    public PlayerJoiningArguments JoiningArguments;
    public string Msg;
    public BattleBitPlayer Player;
    public ulong SteamId;
}

public class GameMode
{
    public string Name = string.Empty;
    protected BattleBitServer R;

    protected GameMode(BattleBitServer r)
    {
        R = r;
    }

    public virtual void Init()
    {
    }

    public virtual void Reset()
    {
        foreach (var player in R.AllPlayers) player.Kill();
    }

    public virtual void OnRoundEnded()
    {
        Reset();
    }

    public virtual OnPlayerKillArguments<BattleBitPlayer> OnAPlayerDownedAnotherPlayer(
        OnPlayerKillArguments<BattleBitPlayer> args)
    {
        return args;
    }

    public virtual BattleBitPlayer OnPlayerGivenUp(BattleBitPlayer player)
    {
        return player;
    }

    public virtual BattleBitPlayer OnPlayerSpawned(BattleBitPlayer player)
    {
        return player;
    }

    public virtual Returner OnPlayerSpawning(BattleBitPlayer player, OnPlayerSpawnArguments request)
    {
        var re = new Returner
        {
            Player = player,
            SpawnArguments = request
        };
        return re;
    }

    public virtual void OnRoundStarted()
    {
    }

    public BattleBitPlayer OnPlayerDisconnected(BattleBitPlayer player)
    {
        return player;
    }

    public Returner OnPlayerJoiningToServer(ulong steamId, PlayerJoiningArguments args)
    {
        var re = new Returner
        {
            SteamId = steamId,
            JoiningArguments = args
        };
        return re;
    }

    public async Task<Returner> OnPlayerTypedMessage(BattleBitPlayer player, ChatChannel channel, string msg)
    {
        var re = new Returner
        {
            Player = player,
            Channel = channel,
            Msg = msg
        };
        return re;
    }
}

public class GameModePlayerData
{

}


public class TeamGunGame : GameMode
{
    public int LevelA;
    public int LevelB;

    public List<WeaponItem> ProgressionList = new()
    {
        new WeaponItem
        {
            Tool = Weapons.FAL,
            MainSight = Attachments.RedDot,
            TopSight = null,
            CantedSight = null,
            Barrel = null,
            SideRail = null,
            UnderRail = null,
            BoltAction = null
        },

        new WeaponItem
        {
            Tool = Weapons.M249,
            MainSight = Attachments.Acog,
            TopSight = null,
            CantedSight = null,
            Barrel = null,
            SideRail = null,
            UnderRail = null,
            BoltAction = null
        },
        new WeaponItem
        {
            Tool = Weapons.M4A1,
            MainSight = Attachments.Holographic,
            TopSight = null,
            CantedSight = Attachments.CantedRedDot,
            Barrel = Attachments.Compensator,
            SideRail = Attachments.Flashlight,
            UnderRail = Attachments.VerticalGrip,
            BoltAction = null
        },
        new WeaponItem
        {
            Tool = Weapons.AK74,
            MainSight = Attachments.RedDot,
            TopSight = Attachments.DeltaSightTop,
            CantedSight = Attachments.CantedRedDot,
            Barrel = Attachments.Ranger,
            SideRail = Attachments.TacticalFlashlight,
            UnderRail = Attachments.Bipod,
            BoltAction = null
        },
        new WeaponItem
        {
            Tool = Weapons.SCARH,
            MainSight = Attachments.Acog,
            TopSight = Attachments.RedDotTop,
            CantedSight = Attachments.Ironsight,
            Barrel = Attachments.MuzzleBreak,
            SideRail = Attachments.TacticalFlashlight,
            UnderRail = Attachments.AngledGrip,
            BoltAction = null
        },
        new WeaponItem
        {
            Tool = Weapons.SSG69,
            MainSight = Attachments._6xScope,
            TopSight = null,
            CantedSight = Attachments.HoloDot,
            Barrel = Attachments.LongBarrel,
            SideRail = Attachments.Greenlaser,
            UnderRail = Attachments.VerticalSkeletonGrip,
            BoltAction = null
        },
        new WeaponItem
        {
            Tool = Weapons.M110,
            MainSight = Attachments.Acog,
            TopSight = Attachments.PistolRedDot,
            CantedSight = Attachments.FYouCanted,
            Barrel = Attachments.Heavy,
            SideRail = Attachments.TacticalFlashlight,
            UnderRail = Attachments.StubbyGrip,
            BoltAction = null
        },
        new WeaponItem
        {
            Tool = Weapons.PP2000,
            MainSight = Attachments.Kobra,
            TopSight = null,
            CantedSight = Attachments.Ironsight,
            Barrel = Attachments.MuzzleBreak,
            SideRail = Attachments.Flashlight,
            UnderRail = Attachments.AngledGrip,
            BoltAction = null
        }
    };

    public TeamGunGame(BattleBitServer r) : base(r)
    {
        Name = "TeamGunGame";
        LevelA = 0;
        LevelB = 0;
    }

    public override Returner OnPlayerSpawning(BattleBitPlayer player, OnPlayerSpawnArguments request)
    {
        var level = 0;
        if (player.Team == Team.TeamA) level = LevelA;
        else if (player.Team == Team.TeamB) level = LevelB;

        request.Loadout.PrimaryWeapon = ProgressionList[level];
        request.Loadout.SecondaryWeapon = default;
        request.Loadout.LightGadget = null;
        request.Loadout.Throwable = null;
        request.Loadout.FirstAid = null;
        request.Loadout.HeavyGadget = new Gadget("Sledge Hammer");
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

    public override OnPlayerKillArguments<BattleBitPlayer> OnAPlayerDownedAnotherPlayer(
        OnPlayerKillArguments<BattleBitPlayer> args)
    {
        args.Victim.Kill();
        int level;
        if (args.Killer.Team == Team.TeamA)
        {
            LevelA++;
            level = LevelA;
        }
        else
        {
            LevelB++;
            level = LevelB;
        }

        if (level == ProgressionList.Count)
        {
            R.AnnounceShort($"{args.Killer.Team.ToString()} only needs 1 more Kill");
        }
        else if (level > ProgressionList.Count)
        {
            R.AnnounceLong($"{args.Killer.Team.ToString()} won the Game");
            R.ForceEndGame();
            Reset();
        }

        return base.OnAPlayerDownedAnotherPlayer(args);
    }


    public override void Reset()
    {
        LevelA = 0;
        LevelB = 0;

        base.Reset();
    }
}


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

public class MeleeOnly : GameMode
{
    public MeleeOnly(BattleBitServer r) : base(r)
    {
        Name = "MeleeOnly";
    }

    public override Returner OnPlayerSpawning(BattleBitPlayer player, OnPlayerSpawnArguments request)
    {
        player.SetLightGadget("Pickaxe", 0, true);
        player.Modifications.RunningSpeedMultiplier = 1.25f;
        player.Modifications.FallDamageMultiplier = 0f;
        player.Modifications.JumpHeightMultiplier = 1.5f;
        return base.OnPlayerSpawning(player, request);
    }
}


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

public class GunGamePlayerData : GameModePlayerData
{
    public Dictionary<ulong, int> Levels = new Dictionary<ulong, int>();


    public int GetLevel(BattleBitPlayer player)
    {
        if (Levels.TryGetValue(player.SteamID, value: out var level)) return level;
        Levels.Add(player.SteamID, 0);
        return 0;

    }

    public void SetLevel(BattleBitPlayer player, int level)
    {
        if (!Levels.TryAdd(player.SteamID, level))
        {
            Levels[player.SteamID] = level;
        }
    }

    public void IncLevel(BattleBitPlayer player)
    {
        var current = GetLevel(player);
        current++;
        SetLevel(player, current);
    }
    public void DecLevel(BattleBitPlayer player)
    {
        var current = GetLevel(player);
        current--;
        SetLevel(player, current);
    }
}

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