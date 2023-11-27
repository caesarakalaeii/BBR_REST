using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using BattleBitAPI.Common;
using BattleBitAPI.Pooling;
using BattleBitAPI.Server;
using ChaosMode.BattleBitCommands;
using Newtonsoft.Json;

namespace ChaosMode.API;

public class BattleBitServer: GameServer<BattleBitPlayer>
{

    public Dictionary<ulong, Broadcaster> BroadcasterList = new();

    public Dictionary<ulong, int> Permissions = new();

    public RedeemHandler RHandler = new RedeemHandler();
    
    

    public void ConsumeCommand(RestEvent restEvent)
    {
        BattleBitPlayer? player;
        Program.Logger.Info($"Command recieved: {restEvent}");
        switch (restEvent.EventType)
        {
            case "AddBroadcaster":
                // add broadcaster to the list if it doesnt exist
                if (BroadcasterList.Keys.Contains(restEvent.SteamId))
                {
                    Program.Logger.Info($"Broadcaster with Id, is already known :)");
                    return;
                }
                BroadcasterList.Add(restEvent.SteamId, new Broadcaster(restEvent.SteamId));
                player = AllPlayers.FirstOrDefault(p => p.SteamID == restEvent.SteamId);
                BroadcasterList[restEvent.SteamId].Player = player;
                WriteSteamIds();
                return;
            case "RemoveBroadcaster":
                // removes broadcaster to the list if it exists
                if (!BroadcasterList.Keys.Contains(restEvent.SteamId))
                {
                    Program.Logger.Info($"Broadcaster with Id, is not known :)");
                    return;
                }
                BroadcasterList.Remove(restEvent.SteamId);
                player = AllPlayers.FirstOrDefault(p => p.SteamID == restEvent.SteamId);
                if (player != null) player.IsBroadcaster = false;
                WriteSteamIds();
                return;
        }

        if (!BroadcasterList.Keys.Contains(restEvent.SteamId))
        {
            Program.Logger.Warn($"Broadcaster with ID {restEvent.SteamId} not known");
            return;
        }
        if(BroadcasterList[restEvent.SteamId].Player == null)
        {
            // if no player instance is known,  try to find the player and set a reference to use
            foreach (var p in AllPlayers) 
            {
                if (BroadcasterList.Keys.Contains(p.SteamID))
                {
                    BroadcasterList[p.SteamID].Player = p;
                }
            }

            if (BroadcasterList[restEvent.SteamId].Player == null) // if player still not found return
            {
                Program.Logger.Warn($"Broadcaster with ID {restEvent.SteamId} not online");
                return;
            }
        }
        switch (restEvent.EventType)
        {

            case "Follow":
                RandomizeRedeem(restEvent);
                break;
            case "Gift":
                RandomizeRedeem(restEvent);

                break;
            case "Sub":
                RandomizeRedeem(restEvent);

                break;
            case "SubBomb":
                RandomizeRedeem(restEvent);

                break;
            case "Raid":
                RandomizeRedeem(restEvent);

                break;
            case "Bits":
                RandomizeRedeem(restEvent);
                break;
            case "Redeem":
                EventHandler(restEvent);
                break;
            
            
                
        }

        
    }

    public void RandomizeRedeem(RestEvent restEvent)
    {
        restEvent.RedeemType = RedeemTypes.DEFAULT;
        while (restEvent.RedeemType == RedeemTypes.DEFAULT)
        {
            restEvent.RedeemType = GenerateRandomRedeem();
        }
        Program.Logger.Info($"Random redeem was chosen to be: {restEvent.RedeemType}");
        EventHandler(restEvent);
    }


    public void EventHandler(RestEvent restEvent)
    {
        BattleBitPlayer? battleBitPlayer;
                switch (restEvent.RedeemType)
                {
                    case RedeemTypes.HEAL:
                        // full heals the player
                        BroadcasterList[restEvent.SteamId].Player?.Heal(100);
                        foreach (var p in AllPlayers)
                        {
                            p.Message($"{BroadcasterList[restEvent.SteamId].Player?.Name} just got healed by {restEvent.Username}! How lucky!", 2);
                        }
                        Program.Logger.Info($"Healed {BroadcasterList[restEvent.SteamId].Player?.Name}({restEvent.SteamId})");
                        break;
                    case RedeemTypes.KILL:
                        // kills the player
                        BroadcasterList[restEvent.SteamId].Player?.Kill();
                        foreach (var p in AllPlayers)
                        {
                            p.Message($"{BroadcasterList[restEvent.SteamId].Player?.Name} just got killed by {restEvent.Username}! How unfortunate!", 2);
                        }
                        Program.Logger.Info($"Killed {BroadcasterList[restEvent.SteamId].Player?.Name}({restEvent.SteamId})");
                        break;
                    case RedeemTypes.SWAP:
                        // switch player with random one (Wierd behavior if player is in save zone)
                        
                        BattleBitPlayer? selectedPlayer = BroadcasterList[restEvent.SteamId].Player;
                        while (selectedPlayer == BroadcasterList[restEvent.SteamId].Player && AllPlayers.Count() > 1)
                        {
                            selectedPlayer = GetRandom<BattleBitPlayer>(AllPlayers);
                        }

                        if (selectedPlayer != null)
                        {
                            battleBitPlayer = BroadcasterList[restEvent.SteamId].Player;
                            if (battleBitPlayer != null)
                                SwapPlayers(battleBitPlayer, selectedPlayer);
                        }
                        Program.Logger.Info($"Swapped {BroadcasterList[restEvent.SteamId].Player?.Name}({restEvent.SteamId}) with {selectedPlayer.Name}({selectedPlayer.SteamID})");
                        foreach (var p in AllPlayers)
                        {
                            p.Message($"Swapped {BroadcasterList[restEvent.SteamId].Player?.Name} with {selectedPlayer.Name}! OwO How exiting!", 2);
                        }
                        break;
                    case RedeemTypes.REVEAL: // Apparently non functional TODO: debug this
                        // show player on map for 1 min
                        battleBitPlayer = BroadcasterList[restEvent.SteamId].Player;
                        if (battleBitPlayer != null)
                        {
                            battleBitPlayer.Modifications.IsExposedOnMap = true;
                            foreach (var p in AllPlayers)
                            {
                                p.Message(
                                    $"{battleBitPlayer?.Name} is now revealed thanks to {restEvent.Username}!", 2);
                            }

                            Program.Logger.Info(
                                $"Revealed {battleBitPlayer?.Name}({restEvent.SteamId})");
                            RHandler.Enqueue(restEvent.RedeemType, async () =>
                            {
                                await Task.Delay(60000);
                                if (battleBitPlayer != null)
                                {
                                    battleBitPlayer.Modifications.IsExposedOnMap = false;

                                }

                            });
                        }
                        break;
                    case RedeemTypes.ZOOMIES:
                        // sets speed to *3 for 1min
                        battleBitPlayer = BroadcasterList[restEvent.SteamId].Player;
                        if (battleBitPlayer != null)
                        {
                            var oldSpeed = battleBitPlayer.Modifications.RunningSpeedMultiplier;
                            battleBitPlayer.Modifications.RunningSpeedMultiplier = oldSpeed * 3;
                            
                            foreach (var p in AllPlayers)
                            {
                                p.Message(
                                    $"{battleBitPlayer?.Name} has the zoomies thanks to {restEvent.Username}!", 2);
                            }
                            
                            RHandler.Enqueue(restEvent.RedeemType, async () =>
                            {
                                await Task.Delay(60000);

                                if (battleBitPlayer != null)
                                    battleBitPlayer.Modifications.RunningSpeedMultiplier = oldSpeed;
                            });
                            
                            Program.Logger.Info(
                                $"Zoomies for {battleBitPlayer?.Name}({restEvent.SteamId})");
                        }
                        break;
                    case RedeemTypes.GLASS:
                        // make player very vulnerable, revert after 30secs
                        battleBitPlayer = BroadcasterList[restEvent.SteamId].Player;
                        if (battleBitPlayer != null)
                        {
                            var oldFallDMG = battleBitPlayer.Modifications.FallDamageMultiplier;
                            var oldRecieveDMG = battleBitPlayer.Modifications.ReceiveDamageMultiplier;
                            battleBitPlayer.Modifications.FallDamageMultiplier = 10;
                            battleBitPlayer.Modifications.ReceiveDamageMultiplier = 10;
                            foreach (var p in AllPlayers)
                            {
                                p.Message(
                                    $"{battleBitPlayer?.Name} is now made of glass, thanks to {restEvent.Username}!", 2);
                            }
                            Program.Logger.Info(
                                $"Glass mode for {battleBitPlayer?.Name}({restEvent.SteamId})");
                            RHandler.Enqueue(restEvent.RedeemType, async () =>
                            {
                                await Task.Delay(30000);

                                if (battleBitPlayer != null)
                                {
                                
                                    battleBitPlayer.Modifications.FallDamageMultiplier = oldFallDMG;
                                    battleBitPlayer.Modifications.ReceiveDamageMultiplier = oldRecieveDMG;
                                }
                                Program.Logger.Info(
                                    $"Glass mode off for {battleBitPlayer?.Name}({restEvent.SteamId})");
                            });
                        }
                        
                        
                        break;
                    case RedeemTypes.FREEZE:
                        //freeze player for 10 secs
                        battleBitPlayer = BroadcasterList[restEvent.SteamId].Player;
                        if (battleBitPlayer != null)
                        {
                            battleBitPlayer.Modifications.Freeze = true;
                            foreach (var p in AllPlayers)
                            {
                                p.Message(
                                    $"{battleBitPlayer?.Name} is now frozen, thanks to {restEvent.Username}!", 2);
                            }
                            Program.Logger.Info(
                                $"Froze {battleBitPlayer?.Name}({restEvent.SteamId})");
                            RHandler.Enqueue(restEvent.RedeemType, async () =>
                            {
                                Program.Logger.Info(
                                    $"we wait");
                                await Task.Delay(10000);
                                Program.Logger.Info(
                                    $"we waited");
                                if (battleBitPlayer != null)
                                {
                                
                                    battleBitPlayer.Modifications.Freeze = false;
                                    Program.Logger.Info(
                                        $"Unfroze {battleBitPlayer?.Name}({restEvent.SteamId})");
                                }
                            });
                        }
                        break;
                    case RedeemTypes.BLEED:
                        // set bleeding to enabled and revert after 1 min
                        
                        battleBitPlayer = BroadcasterList[restEvent.SteamId].Player;
                        if (battleBitPlayer != null)
                        {
                            var oldMinDmgBleed = battleBitPlayer.Modifications.MinimumDamageToStartBleeding;
                            var oldMinHpBleed = battleBitPlayer.Modifications.MinimumHpToStartBleeding;
                            battleBitPlayer.Modifications.EnableBleeding(0, 100);
                            foreach (var p in AllPlayers)
                            {
                                p.Message(
                                    $"{battleBitPlayer?.Name} is now bleeding, thanks to {restEvent.Username}!", 2);
                            }
                            RHandler.Enqueue((RedeemTypes)restEvent.RedeemType, async () =>
                            {
                                await Task.Delay(60000);

                                battleBitPlayer?.Modifications.EnableBleeding(oldMinHpBleed, oldMinDmgBleed);
                            });
                        }
                        
                        Program.Logger.Info(
                            $"Bleed {battleBitPlayer?.Name}({restEvent.SteamId})");
                        break;
                    case RedeemTypes.TRUNTABLES:
                        // switches Team
                        BroadcasterList[restEvent.SteamId].Player?.ChangeTeam();
                        
                        foreach (var p in AllPlayers)
                        {
                            p.Message($"{BroadcasterList[restEvent.SteamId].Player?.Name} just switched teams, by {restEvent.Username}! How the turntables!", 2);
                        }
                        Program.Logger.Info($"Truntabled {BroadcasterList[restEvent.SteamId].Player?.Name}({restEvent.SteamId})");
                        break;
                    
                    case RedeemTypes.MEELEE:
                        // set gadget to Pickaxe and clears previous Loadout
                        // Needs Queuing???
                        BroadcasterList[restEvent.SteamId].Player?.SetLightGadget("Pickaxe", 0, true);
                        foreach (var p in AllPlayers)
                        {
                            p.Message($"{BroadcasterList[restEvent.SteamId].Player?.Name} just went commando thanks {restEvent.Username}! How the turntables!", 2);
                        }
                        Program.Logger.Info($"Melee Only {BroadcasterList[restEvent.SteamId].Player?.Name}({restEvent.SteamId})");
                        break;
                    
                        
                    
                    // Enums are there, Twitch and BattleBit parts need to be added
                    
                    // zoomies 4 all
                    // ammo set ammo to 0?
                        
                        
                }

                if (!RHandler.IsRunning)
                { // spawn new redeem queue instance if old one is not running
                    Program.Logger.Info($"Spawning new Handler for {restEvent.RedeemType}");
                    Task.Run(() => { RHandler.Run(restEvent.RedeemType!); });
                }
    }

    public RedeemTypes GenerateRandomRedeem()
    {
        Array enumValues = Enum.GetValues(typeof(RedeemTypes));
        Random random = new Random();
        return (RedeemTypes)(enumValues.GetValue(random.Next(enumValues.Length)) ?? RedeemTypes.DEFAULT);
        
    }

    public static void SwapPlayers(BattleBitPlayer player1, BattleBitPlayer player2)
    {
        Vector3 pos1 = player1.Position;
        Vector3 pos2 = player2.Position;
        player1.Teleport(pos2);
        player2.Teleport(pos1);
    }
    
    
    static T GetRandom<T>(IEnumerable<T> enumerable)
    {
        Random random = new Random();
        int randomIndex = random.Next(0, enumerable.Count());
        return enumerable.ElementAt(randomIndex);
    }
    
    public void WriteSteamIds()
    {
        Directory.CreateDirectory(Path.GetDirectoryName("data/broadcasters.json") ?? string.Empty);
        List<ulong> ulongList = new List<ulong>();
        foreach (var broadcaster in BroadcasterList)
        {
            ulongList.Add(broadcaster.Key);
        }
        
        string json = JsonConvert.SerializeObject(ulongList, Formatting.Indented);
        System.IO.File.WriteAllText("data/broadcasters.json", json);
        Program.Logger.Info("Saved Broadcasters");
    }

    public void LoadSteamIds()
    {
        // Read the JSON from the file
        
        if (!File.Exists("data/broadcasters.json"))
        {
            Program.Logger.Warn("No Broadcasters to load");
            return;
        }

        string json = System.IO.File.ReadAllText("data/broadcasters.json");
        // Deserialize the JSON to a list of ulong
        List<ulong> ulongList = JsonConvert.DeserializeObject<List<ulong>>(json);

        foreach (var steamId in ulongList)
        {
            BroadcasterList.Add(steamId, (new Broadcaster(steamId)));
        }
        
        Program.Logger.Info("Loaded Broadcasters");
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
        LoadSteamIds();
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

    public override Task OnSessionChanged(long oldSessionID, long newSessionID)
    {
        foreach (var player in AllPlayers)
        {
            if (BroadcasterList.Keys.Contains(player.SteamID))
            {
                BroadcasterList[player.SteamID].Player = player;
            }
        }
        return base.OnSessionChanged(oldSessionID, newSessionID);
    }

    public override Task OnPlayerConnected(BattleBitPlayer player)
    {
        if (BroadcasterList.Keys.Contains(player.SteamID))
        {
            BroadcasterList[player.SteamID].Player = player;
        }

        if (!Permissions.Keys.Contains(player.SteamID))
        {
            Permissions[player.SteamID] = 0;
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

public class RedeemHandler
{
    private Dictionary<RedeemTypes, Queue<Func<Task>>> RedeemQueues = new(); 
    private readonly Array _availableRedeems = Enum.GetValues(typeof(RedeemTypes));
    public bool IsRunning;
    public RedeemHandler()
    {
        //build necessary queues
        IsRunning = false;
        foreach (var enumValue in _availableRedeems)
        {
            RedeemQueues[(RedeemTypes)enumValue] = new Queue<Func<Task>>();
        }
    }

    public async void Run(RedeemTypes redeemType)
    {
        IsRunning = true;
        while (IsRunning)
        {
            if (RedeemQueues[redeemType].Count == 0)
            {
                IsRunning = false;
                Program.Logger.Info($"Killing task for {redeemType}");
                return;
            }
            Func<Task> func = RedeemQueues[redeemType].Dequeue();
            await Task.Run(func);
        }
    }

    public void Enqueue(RedeemTypes redeemType, Func<Task> func)
    {
        
        RedeemQueues[redeemType].Enqueue(func);
    }
    
}
public enum RedeemTypes
{

    HEAL,
    KILL,
    SWAP,
    REVEAL,
    ZOOMIES,
    GLASS,
    FREEZE,
    BLEED,
    TRUNTABLES,
    MEELEE, 
    DEFAULT
    
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