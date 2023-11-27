using System.Threading.Tasks;
using BattleBitAPI.Common;

namespace ChaosMode.API;

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