using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using ChaosMode.BattleBitCommands;
using ChaosMode.Modules;
using Newtonsoft.Json;

namespace ChaosMode.API;

public class BattleBitServer: GameServer<BattleBitPlayer>
{

    public Dictionary<ulong, Broadcaster> BroadcasterList = new();

    public Dictionary<ulong, int> Permissions = new();
    
    public readonly List<GameMode> GameModes;
    public readonly List<InGameCommand> InGameCommands;
    public Dictionary<ulong, RedeemHandler> RedeemHandlers = new();
    public GameMode CurrentGameMode;
    public bool CyclePlaylist;
    public int GameModeIndex;

    public readonly List<ServerModule> ServerModules;

    
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
        ServerModules = new List<ServerModule>
        {
            new ChatRewrite(this),
            new PlayerRoles(this),
            new IllegalPlayerActions(this),
            new LoadingScreenText(this)
        };
        
        
        
        GameModeIndex = 0;
        CurrentGameMode = GameModes[GameModeIndex];
        LoadSteamIds();
        foreach (var broadcaster in BroadcasterList.Keys)
        {
            RedeemHandlers[broadcaster] = new RedeemHandler();
            Permissions[broadcaster] = 20;
        }
        Permissions[76561198053896127] = 50; // Add Admin perms for caesar
    }


    

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
                for (int i = 0; i < restEvent.Tier; i++)
                {
                    RandomizeRedeem(restEvent);
                }
                break;
            case "Sub":
                for (int i = 0; i < restEvent.Tier; i++)
                {
                    RandomizeRedeem(restEvent);
                }
                break;
            case "SubBomb":
                for (int i = 0; i < restEvent.Amount; i++)
                {
                    for (int j = 0; j < restEvent.Tier; j++)
                    {
                        RandomizeRedeem(restEvent);
                    }
                }
                break;
            case "Raid":
                for (int i = 0; i < restEvent.Amount/10; i++)
                {
                    RandomizeRedeem(restEvent);
                };
                break;
            case "Bits":
                for (int i = 0; i < restEvent.Amount/10; i++)
                {
                    RandomizeRedeem(restEvent);
                };
                break;
            case "Redeem":
                if(restEvent.RedeemType == RedeemTypes.RANDOM) RandomizeRedeem(restEvent);
                else EventHandler(restEvent);
                break;
            case "Random":
                RandomizeRedeem(restEvent);
                break;
        }

        //todo: add chat message on event end
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
        RedeemHandler rHandler = RedeemHandlers[restEvent.SteamId];
        
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
                            rHandler.Enqueue(restEvent.RedeemType, async () =>
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
                            
                            rHandler.Enqueue(restEvent.RedeemType, async () =>
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
                            rHandler.Enqueue(restEvent.RedeemType, async () =>
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
                            rHandler.Enqueue(restEvent.RedeemType, async () =>
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
                            battleBitPlayer.Modifications.EnableBleeding(100, 0);
                            foreach (var p in AllPlayers)
                            {
                                p.Message(
                                    $"{battleBitPlayer?.Name} is now bleeding, thanks to {restEvent.Username}!", 2);
                            }
                            rHandler.Enqueue((RedeemTypes)restEvent.RedeemType, async () =>
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

                if (!rHandler.IsRunning && BroadcasterList[restEvent.SteamId].ChaosEnabled)
                { // spawn new redeem queue instance if old one is not running
                    Program.Logger.Info($"Spawning new Handler for {restEvent.RedeemType}");
                    Task.Run(() => { rHandler.Run(restEvent.RedeemType!); });
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


    




    //GAMEMODE AND MODULE PASSTHROUGH
    
    public void AddEvent(ServerModule @module, BattleBitServer server)
    {
        @module.Server = server;
        
        ServerModules.Add(@module);
    }

    public void RemoveEvent(ServerModule @module)
    {
        if (!ServerModules.Contains(@module))
            return;

        ServerModules.Remove(@module);
    }

    public override async Task OnConnected()
    {
        foreach (var @module in ServerModules)
            await @module.OnConnected();
    }

    public override async Task OnTick()
    {
        foreach (var @module in ServerModules)
            await @module.OnTick();
    }

    public override async Task OnDisconnected()
    {
        foreach (var @module in ServerModules)
            await @module.OnDisconnected();
    }

    public override async Task OnPlayerConnected(BattleBitPlayer player)
    {
        if (BroadcasterList.Keys.Contains(player.SteamID))
        {
            BroadcasterList[player.SteamID].Player = player;
        }

        if (!Permissions.Keys.Contains(player.SteamID))
        {
            Permissions[player.SteamID] = 0;
        }
        foreach (var @module in ServerModules)
            await @module.OnPlayerConnected(player);
    }

    public override async Task OnPlayerDisconnected(BattleBitPlayer player)
    {
        if (BroadcasterList.Keys.Contains(player.SteamID))
        {
            BroadcasterList[player.SteamID].Player = null;
        }
        CurrentGameMode.OnPlayerDisconnected(player);
        
        foreach (var @module in ServerModules)
            await @module.OnPlayerDisconnected(player);
    }

    public override async Task<bool> OnPlayerTypedMessage(BattleBitPlayer player, ChatChannel channel, string msg)
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

            return false;
        }

        await CurrentGameMode.OnPlayerTypedMessage(player, channel, msg);
        foreach (var @module in ServerModules)
            if (!await @module.OnPlayerTypedMessage(player, channel, msg))
                return false;

        return true;
    }

    public override async Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args)
    {
        foreach (var @module in ServerModules)
            await @module.OnPlayerJoiningToServer(steamID, args);
    }

    public override async Task OnSavePlayerStats(ulong steamID, PlayerStats stats)
    {
        foreach (var @module in ServerModules)
            await @module.OnSavePlayerStats(steamID, stats);
    }

    public override async Task<bool> OnPlayerRequestingToChangeRole(BattleBitPlayer player, GameRole requestedRole)
    {
        foreach (var @module in ServerModules)
            if (!await @module.OnPlayerRequestingToChangeRole(player, requestedRole))
                return false;

        return true;
    }

    public override async Task<bool> OnPlayerRequestingToChangeTeam(BattleBitPlayer player, Team requestedTeam)
    {
        foreach (var @module in ServerModules)
            if (!await @module.OnPlayerRequestingToChangeTeam(player, requestedTeam))
                return false;

        return true;
    }

    public override async Task OnPlayerChangedRole(BattleBitPlayer player, GameRole role)
    {
        foreach (var @module in ServerModules)
            await @module.OnPlayerChangedRole(player, role);
    }

    public override async Task OnPlayerJoinedSquad(BattleBitPlayer player, Squad<BattleBitPlayer> squad)
    {
        foreach (var @module in ServerModules)
            await @module.OnPlayerJoinedSquad(player, squad);
    }

    public override async Task OnSquadLeaderChanged(Squad<BattleBitPlayer> squad, BattleBitPlayer newLeader)
    {
        foreach (var @module in ServerModules)
            await @module.OnSquadLeaderChanged(squad, newLeader);
    }

    public override async Task OnPlayerLeftSquad(BattleBitPlayer player, Squad<BattleBitPlayer> squad)
    {
        foreach (var @module in ServerModules)
            await @module.OnPlayerLeftSquad(player, squad);
    }
    
    public override async Task OnPlayerChangeTeam(BattleBitPlayer player, Team team)
    {
        foreach (var @module in ServerModules)
            await @module.OnPlayerChangeTeam(player, team);
    }
    
    public override async Task OnSquadPointsChanged(Squad<BattleBitPlayer> squad, int newPoints)
    {
        foreach (var @module in ServerModules)
            await @module.OnSquadPointsChanged(squad, newPoints);
    }
    
    public override async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitPlayer player, OnPlayerSpawnArguments request)
    {
        CurrentGameMode.OnPlayerSpawning(player, request);
        foreach (var @module in ServerModules)
            await @module.OnPlayerSpawning(player, request);

        return request;
    }
    
    public override async Task OnPlayerSpawned(BattleBitPlayer player)
    {
        var ret = CurrentGameMode.OnPlayerSpawned(player);
        foreach (var @module in ServerModules)
            await @module.OnPlayerSpawned(player);
    }
    
    public override async Task OnPlayerDied(BattleBitPlayer player)
    {
        foreach (var @module in ServerModules)
            await @module.OnPlayerDied(player);
    }
    
    public override async Task OnPlayerGivenUp(BattleBitPlayer player)
    {
        var ret = CurrentGameMode.OnPlayerGivenUp(player);
        foreach (var @module in ServerModules)
            await @module.OnPlayerGivenUp(player);
    }
    
    public override async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<BattleBitPlayer> args)
    {
        var ret = CurrentGameMode.OnAPlayerDownedAnotherPlayer(args);
        foreach (var @module in ServerModules)
            await @module.OnAPlayerDownedAnotherPlayer(args);
    }
    
    public override async Task OnAPlayerRevivedAnotherPlayer(BattleBitPlayer from, BattleBitPlayer to)
    {
        foreach (var @module in ServerModules)
            await @module.OnAPlayerRevivedAnotherPlayer(from, to);
    }
    
    public override async Task OnPlayerReported(BattleBitPlayer from, BattleBitPlayer to, ReportReason reason, string additional)
    {
        foreach (var @module in ServerModules)
            await @module.OnPlayerReported(from, to, reason, additional);
    }
    
    public override async Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        foreach (var @module in ServerModules)
            await @module.OnGameStateChanged(oldState, newState);
    }
    
    public override async Task OnRoundStarted()
    {
        foreach (var @module in ServerModules)
            await @module.OnRoundStarted();
    }
    
    public override async Task OnRoundEnded()
    {
        foreach (var @module in ServerModules)
            await @module.OnRoundEnded();
    }
    
    public override async Task OnSessionChanged(long oldSessionID, long newSessionID)
    {
        foreach (var player in AllPlayers)
        {
            if (BroadcasterList.Keys.Contains(player.SteamID))
            {
                BroadcasterList[player.SteamID].Player = player;
            }
        }
        
        CurrentGameMode.OnSessionChanged(oldSessionID, newSessionID);
        foreach (var @module in ServerModules)
            await @module.OnSessionChanged(oldSessionID, newSessionID);
    }
    
    
    
  
  
    

 
}