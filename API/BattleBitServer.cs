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
    public BattleBitRest Rest { get; set; }

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
            new AddPermissionCommand(this),
            new ToggleRedeemsCommand(this),
            new ToggleVoteCommand(this)
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
            RedeemHandlers[broadcaster] = new RedeemHandler(this, BroadcasterList[broadcaster].Player);
            Permissions[broadcaster] = 20;
        }
        Permissions[76561198053896127] = 50; // Add Admin perms for caesar
        
        StartRest();
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
            RedeemHandlers[player.SteamID].Player = player;
            Rest.StartVotesREST(BroadcasterList[player.SteamID]);
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
            Rest.StopVotesREST(BroadcasterList[player.SteamID]);
        }

        if (RedeemHandlers.Keys.Contains(player.SteamID))
        {
            RedeemHandlers[player.SteamID].Vote.Player = null;
            
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
    
    
    private void StartRest()
    {
        Program.Logger.Info("Starting REST API");
        BattleBitRest rest = new BattleBitRest(Program.ServerConfiguration.RestPort);
        Rest = rest;
        rest.Run();
    }
    
    
    
  
  
    

 
}