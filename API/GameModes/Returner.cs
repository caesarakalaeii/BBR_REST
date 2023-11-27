using BattleBitAPI.Common;

namespace ChaosMode.API;

public class Returner
{
    public OnPlayerSpawnArguments SpawnArguments;
    public ChatChannel Channel;
    public PlayerJoiningArguments JoiningArguments;
    public string Msg;
    public BattleBitPlayer Player;
    public ulong SteamId;
}