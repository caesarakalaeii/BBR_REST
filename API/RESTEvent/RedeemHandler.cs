using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using BattleBitAPI.Common;
using ChaosMode.Modules;

namespace ChaosMode.API.RESTEvent;

public class RedeemHandler
{
    
    
    private Dictionary<RedeemTypes, Queue<Func<Task>>> RedeemQueues = new();
    public Vote Vote;
    private readonly Array _availableRedeems = Enum.GetValues(typeof(RedeemTypes));
    public bool IsRunning;
    public BattleBitServer Server;
    public BattleBitPlayer? Player;
    public RedeemHandler(BattleBitServer server, BattleBitPlayer? player)
    {
        //build necessary queues
        Player = player;
        Server = server;
        IsRunning = false;
        Vote = new Vote();
        foreach (var enumValue in _availableRedeems)
        {
            RedeemQueues[(RedeemTypes)enumValue] = new Queue<Func<Task>>();
        }
    }

    public async void Run(RedeemTypes redeemType)
    {
        if(Player == null) return;
        IsRunning = true;
        while (IsRunning)
        {
            while (!Player.IsAlive && RedeemQueues[redeemType].Count != 0 )
            {
                await Task.Delay(1);
            }
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
    
     

    public void ConsumeCommand(RestEvent restEvent)
    {
        BattleBitPlayer? player;
        Program.Logger.Info($"Command recieved: {restEvent}");
        switch (restEvent.EventType)
        {
            case "AddBroadcaster":
                // add broadcaster to the list if it doesnt exist
                if (Server.BroadcasterList.Keys.Contains(restEvent.SteamId))
                {
                    Program.Logger.Info($"Broadcaster with Id, is already known :)");
                    return;
                }
                Server.BroadcasterList.Add(restEvent.SteamId, new Broadcaster(restEvent.SteamId));
                player = Server.AllPlayers.FirstOrDefault(p => p.SteamID == restEvent.SteamId);
                Server.BroadcasterList[restEvent.SteamId].Player = player;
                Server.WriteSteamIds();
                return;
            case "RemoveBroadcaster":
                // removes broadcaster to the list if it exists
                if (!Server.BroadcasterList.Keys.Contains(restEvent.SteamId))
                {
                    Program.Logger.Info($"Broadcaster with Id, is not known :)");
                    return;
                }
                Server.BroadcasterList.Remove(restEvent.SteamId);
                player = Server.AllPlayers.FirstOrDefault(p => p.SteamID == restEvent.SteamId);
                if (player != null) player.IsBroadcaster = false;
                Server.WriteSteamIds();
                return;
        }

        if (!Server.BroadcasterList.Keys.Contains(restEvent.SteamId))
        {
            Program.Logger.Warn($"Broadcaster with ID {restEvent.SteamId} not known");
            return;
        }
        if(Server.BroadcasterList[restEvent.SteamId].Player == null)
        {
            // if no player instance is known,  try to find the player and set a reference to use
            foreach (var p in Server.AllPlayers) 
            {
                if (Server.BroadcasterList.Keys.Contains(p.SteamID))
                {
                    Server.BroadcasterList[p.SteamID].Player = p;
                }
            }

            if (Server.BroadcasterList[restEvent.SteamId].Player == null) // if player still not found return
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
                if(!Server.BroadcasterList[restEvent.SteamId].AcceptsRedeems) return; // return if no redeems are accepted
                
                if(restEvent.RedeemType == RedeemTypes.RANDOM) RandomizeRedeem(restEvent);
                else EventHandler(restEvent);
                break;
            case "Random":
                RandomizeRedeem(restEvent);
                break;
            case "VoteOnGoing":
                if (Server.BroadcasterList[restEvent.SteamId].AcceptsVotes && !Vote.isOnGoing)
                {
                    Vote = new Vote();
                    Vote.Player = Server.BroadcasterList[restEvent.SteamId].Player;
                    Vote.StartVote();
                }
                Vote.UpdateVote(restEvent);
                break;
            case "VoteEnd":
                Vote.EndVote(restEvent);
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

        if (!Server.BroadcasterList[restEvent.SteamId].AcceptsRedeems &&
            !Server.BroadcasterList[restEvent.SteamId].AcceptsVotes) return;
        
        switch (restEvent.RedeemType)
            {
                case RedeemTypes.HEAL:
                    //heals the player
                    Heal(restEvent);
                    break;
                case RedeemTypes.KILL:
                    // kills the player
                    Kill(restEvent);
                    break;
                case RedeemTypes.SWAP:
                    // switch player with random one (Wierd behavior if player is in save zone)
                    Swap(restEvent);
                    break;
                case RedeemTypes.REVEAL: // Apparently non functional TODO: debug this
                    Reveal(restEvent);
                    break;
                case RedeemTypes.ZOOMIES:
                    Zoomies(restEvent);
                    break;
                case RedeemTypes.GLASS:
                    Glass(restEvent);
                    break;
                case RedeemTypes.FREEZE:
                    Freeze(restEvent);
                    break;
                case RedeemTypes.BLEED:
                    Bleed(restEvent);
                    break;
                case RedeemTypes.TRUNTABLES:
                    TurnTables(restEvent);
                    break;
                case RedeemTypes.MEELEE:
                    Melee(restEvent);
                    break;
                
                    
                
                // Enums are there, Twitch and BattleBit parts need to be added
                
                // zoomies 4 all
                // ammo set ammo to 0?
                // disable UI
                    
                    
            }

            if (!IsRunning && Server.BroadcasterList[restEvent.SteamId].ChaosEnabled)
            { // spawn new redeem queue instance if old one is not running
                Program.Logger.Info($"Spawning new Handler for {restEvent.RedeemType}");
                Task.Run(() => {Run(restEvent.RedeemType!); });
            }
    }

    public static RedeemTypes GenerateRandomRedeem()
    {
        Array enumValues = Enum.GetValues(typeof(RedeemTypes));
        Random random = new Random();
        var redeem = RedeemTypes.DEFAULT;
        while (redeem == RedeemTypes.DEFAULT)
        {
            redeem = (RedeemTypes)(enumValues.GetValue(random.Next(enumValues.Length)) ?? RedeemTypes.DEFAULT);
        }

        return redeem;
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


    public void TurnTables(RestEvent restEvent)
    {
        // switches Team
        Server.BroadcasterList[restEvent.SteamId].Player?.ChangeTeam();
                        
        foreach (var p in Server.AllPlayers)
        {
            p.Message($"{Server.BroadcasterList[restEvent.SteamId].Player?.Name} just switched teams, by {restEvent.Username}! How the turntables!", 2);
        }
        Program.Logger.Info($"Truntabled {Server.BroadcasterList[restEvent.SteamId].Player?.Name}({restEvent.SteamId})");
    }

    public void Melee(RestEvent restEvent)
    {
        // set gadget to Pickaxe and clears previous Loadout 
        Enqueue(restEvent.RedeemType, async () =>
        {
            if (Player != null)
            {
                var oldLoadOut = Player.CurrentLoadout;
                
                
                
                

            Server.BroadcasterList[restEvent.SteamId].Player?.SetLightGadget("Pickaxe", 0, true);
            foreach (var p in Server.AllPlayers)
            {
                p.Message(
                    $"{Server.BroadcasterList[restEvent.SteamId].Player?.Name} just went commando thanks {restEvent.Username}! Watch your Back!",
                    2);
            }
            Program.Logger.Info(
                $"Melee Only {Server.BroadcasterList[restEvent.SteamId].Player?.Name}({restEvent.SteamId})");
            await Task.Delay(30000);
            
            UpdateLoadout(Player, oldLoadOut); // reset loadout to old one
            Player?.Message("Have fun with your old Loadout", 2);

            
            }
        });
    }
    

    public void Bleed(RestEvent restEvent)
    {
        // set bleeding to enabled and revert after 1 min
                        
        Enqueue((RedeemTypes)restEvent.RedeemType, async () =>
        {
            if (Player != null)
            {
                var oldMinDmgBleed = Player.Modifications.MinimumDamageToStartBleeding;
                var oldMinHpBleed = Player.Modifications.MinimumHpToStartBleeding;
                Player.Modifications.EnableBleeding(100, 0);
                foreach (var p in Server.AllPlayers)
                {
                    p.Message(
                        $"{Player?.Name} is now bleeding, thanks to {restEvent.Username}!", 2);
                }

                await Task.Delay(60000);
                Player?.Message("Bleeding has Ended", 2);
                Player?.Modifications.EnableBleeding(oldMinHpBleed, oldMinDmgBleed);
            }
        });
        
                        
        Program.Logger.Info(
            $"Bleed {Player?.Name}({restEvent.SteamId})");
    }

    public void Freeze(RestEvent restEvent)
    {
        if (Player != null)
        {
            Enqueue(restEvent.RedeemType, async () =>
            {
            Player.Modifications.Freeze = true;
            foreach (var p in Server.AllPlayers)
            {
                p.Message(
                    $"{Player?.Name} is now frozen, thanks to {restEvent.Username}!", 2);
            }
            Program.Logger.Info(
                $"Froze {Player?.Name}({restEvent.SteamId})");
                
                await Task.Delay(10000);
                if (Player != null)
                {
                                
                    Player.Modifications.Freeze = false;
                    Player.Message("Freeze has Ended", 2);
                    Program.Logger.Info(
                        $"Unfroze {Player?.Name}({restEvent.SteamId})");
                }
            });
        }
    }

    public void Glass(RestEvent restEvent)
    {
        // make player very vulnerable, revert after 30secs
        if (Player != null)
        {
            Enqueue(restEvent.RedeemType, async () =>
            {
                var oldFallDMG = Player.Modifications.FallDamageMultiplier;
                var oldRecieveDMG = Player.Modifications.ReceiveDamageMultiplier;
                Player.Modifications.FallDamageMultiplier = 10;
                Player.Modifications.ReceiveDamageMultiplier = 10;
                foreach (var p in Server.AllPlayers)
                {
                    p.Message(
                        $"{Player?.Name} is now made of glass, thanks to {restEvent.Username}!", 2);
                }
                Program.Logger.Info(
                    $"Glass mode for {Player?.Name}({restEvent.SteamId})");
                await Task.Delay(30000);

                if (Player != null)
                {
                                
                    Player.Modifications.FallDamageMultiplier = oldFallDMG;
                    Player.Modifications.ReceiveDamageMultiplier = oldRecieveDMG;
                    Player.Message("Glass mode has Ended", 2);
                }
                Program.Logger.Info(
                    $"Glass mode off for {Player?.Name}({restEvent.SteamId})");
            });
        }
    }

    public void Zoomies(RestEvent restEvent)
    {
        // sets speed to *3 for 15 secs
        if (Player != null)
        {
            Enqueue(restEvent.RedeemType, async () =>
            {
                var oldSpeed = Player.Modifications.RunningSpeedMultiplier;
                Player.Modifications.RunningSpeedMultiplier = oldSpeed * 3;

                foreach (var p in Server.AllPlayers)
                {
                    p.Message(
                        $"{Player?.Name} has the zoomies thanks to {restEvent.Username}!", 2);
                }

                Program.Logger.Info(
                    $"Zoomies for {Player?.Name}({restEvent.SteamId})");
                await Task.Delay(15000);
                if(Player == null)return;
                
                Player.Modifications.RunningSpeedMultiplier = oldSpeed;
                Player.Message("Zoomies have Ended", 2);
            });

        }
    }

    public void Reveal(RestEvent restEvent)
    {
        // show player on map for 1 min
        if (Player != null)
        {
            Enqueue(restEvent.RedeemType, async () =>
            {
            Player.Modifications.IsExposedOnMap = true;
            foreach (var p in Server.AllPlayers)
            {
                p.Message(
                    $"{Player?.Name} is now revealed thanks to {restEvent.Username}!", 2);
            }

            Program.Logger.Info(
                $"Revealed {Player?.Name}({restEvent.SteamId})");
                await Task.Delay(60000);
                if (Player == null) return;
                
                Player.Modifications.IsExposedOnMap = false;
                Player.Message("You are no longer Revealed", 2);
                
                

            });
        }
    }

    public void Swap(RestEvent restEvent)
    {
        Enqueue(restEvent.RedeemType, async () =>
        {
            BattleBitPlayer? selectedPlayer = Server.BroadcasterList[restEvent.SteamId].Player;
            while (selectedPlayer == Server.BroadcasterList[restEvent.SteamId].Player && Server.AllPlayers.Count() > 1)
            {
                selectedPlayer = GetRandom(Server.AllPlayers);
            }

            if (selectedPlayer != null)
            {
                Player = Server.BroadcasterList[restEvent.SteamId].Player;
                if (Player != null)
                    SwapPlayers(Player, selectedPlayer);
                Program.Logger.Info(
                    $"Swapped {Server.BroadcasterList[restEvent.SteamId].Player?.Name}({restEvent.SteamId}) with {selectedPlayer.Name}({selectedPlayer.SteamID})");
                foreach (var p in Server.AllPlayers)
                {
                    p.Message(
                        $"Swapped {Server.BroadcasterList[restEvent.SteamId].Player?.Name} with {selectedPlayer.Name}! OwO How exiting!",
                        2);
                }
            }
        });
    }

    public void Kill(RestEvent restEvent)
    {
        Enqueue(restEvent.RedeemType, async () =>
        {
            Server.BroadcasterList[restEvent.SteamId].Player?.Kill();
            foreach (var p in Server.AllPlayers)
            {
                p.Message(
                    $"{Server.BroadcasterList[restEvent.SteamId].Player?.Name} just got killed by {restEvent.Username}! How unfortunate!",
                    2);
            }

            Program.Logger.Info(
                $"Killed {Server.BroadcasterList[restEvent.SteamId].Player?.Name}({restEvent.SteamId})");
        });
    }

    public void Heal(RestEvent restEvent)
    {
        // full heals the player
        Enqueue(restEvent.RedeemType, async () =>
        {
            Server.BroadcasterList[restEvent.SteamId].Player?.Heal(100);
            foreach (var p in Server.AllPlayers)
            {
                p.Message(
                    $"{Server.BroadcasterList[restEvent.SteamId].Player?.Name} just got healed by {restEvent.Username}! How lucky!",
                    2);
            }

            Program.Logger.Info(
                $"Healed {Server.BroadcasterList[restEvent.SteamId].Player?.Name}({restEvent.SteamId})");
        });
    }
    
    private static void UpdateLoadout(BattleBitPlayer player, PlayerLoadout loadout)
    {
        player.SetLightGadget(loadout.LightGadgetName, loadout.LightGadgetExtra, true);
        player.SetThrowable(loadout.ThrowableName, loadout.ThrowableExtra);
        player.SetHeavyGadget(loadout.HeavyGadgetName, loadout.HeavyGadgetExtra);
        player.SetFirstAidGadget(loadout.FirstAidName, loadout.FirstAidExtra);
        player.SetSecondaryWeapon(loadout.SecondaryWeapon, loadout.SecondaryExtraMagazines);
        player.SetPrimaryWeapon(loadout.PrimaryWeapon, loadout.PrimaryExtraMagazines);
    }
}

