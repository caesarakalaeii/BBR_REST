using System.Collections.Generic;

namespace ChaosMode.API;

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