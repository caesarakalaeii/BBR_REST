namespace ChaosMode.API;

public class Broadcaster
{
    public Broadcaster(ulong steamId)
    {
        SteamId = steamId;
        Player = null;
    }
    
    public BattleBitPlayer? Player { get; set; }

    public ulong SteamId { get; set; }
    
}