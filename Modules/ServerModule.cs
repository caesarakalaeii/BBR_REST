using System.Threading.Tasks;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using ChaosMode.API;
using log4net;

namespace ChaosMode;

public abstract class ServerModule
{
    /// <summary>
    /// Stolen from https://github.com/DasIschBims/Lifesteal
    /// </summary>

    public BattleBitServer Server;

    public ServerModule(BattleBitServer server)
    {
        Server = server;
    }
    public virtual async Task OnConnected()
    {
    }

    public virtual async Task OnTick()
    {
    }

    public virtual async Task OnDisconnected()
    {
    }

    public virtual async Task OnPlayerConnected(BattleBitPlayer player)
    {
    }

    public virtual async Task OnPlayerDisconnected(BattleBitPlayer player)
    {
    }

    public virtual async Task<bool> OnPlayerTypedMessage(BattleBitPlayer player, ChatChannel channel, string msg)
    {
        return true;
    }

    public virtual async Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args)
    {
    }

    public virtual async Task OnSavePlayerStats(ulong steamID, PlayerStats stats)
    {
    }

    public virtual async Task<bool> OnPlayerRequestingToChangeRole(BattleBitPlayer player, GameRole requestedRole)
    {
        return true;
    }

    public virtual async Task<bool> OnPlayerRequestingToChangeTeam(BattleBitPlayer player, Team requestedTeam)
    {
        return true;
    }

    public virtual async Task OnPlayerChangedRole(BattleBitPlayer player, GameRole role)
    {
    }

    public virtual async Task OnPlayerJoinedSquad(BattleBitPlayer player, Squad<BattleBitPlayer> squad)
    {
    }

    public virtual async Task OnSquadLeaderChanged(Squad<BattleBitPlayer> squad, BattleBitPlayer newLeader)
    {
    }

    public virtual async Task OnPlayerLeftSquad(BattleBitPlayer player, Squad<BattleBitPlayer> squad)
    {
    }

    public virtual async Task OnPlayerChangeTeam(BattleBitPlayer player, Team team)
    {
    }

    public virtual async Task OnSquadPointsChanged(Squad<BattleBitPlayer> squad, int newPoints)
    {
    }

    public virtual async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitPlayer player, OnPlayerSpawnArguments request)
    {
        return request;
    }

    public virtual async Task OnPlayerSpawned(BattleBitPlayer player)
    {
    }

    public virtual async Task OnPlayerDied(BattleBitPlayer player)
    {
    }

    public virtual async Task OnPlayerGivenUp(BattleBitPlayer player)
    {
    }

    public virtual async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<BattleBitPlayer> args)
    {
    }

    public virtual async Task OnAPlayerRevivedAnotherPlayer(BattleBitPlayer from, BattleBitPlayer to)
    {
    }

    public virtual async Task OnPlayerReported(BattleBitPlayer from, BattleBitPlayer to, ReportReason reason,
        string additional)
    {
    }

    public virtual async Task OnGameStateChanged(GameState oldState, GameState newState)
    {
    }

    public virtual async Task OnRoundStarted()
    {
    }

    public virtual async Task OnRoundEnded()
    {
    }

    public virtual async Task OnSessionChanged(long oldSessionID, long newSessionID)
    {
    }
}