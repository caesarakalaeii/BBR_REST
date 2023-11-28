namespace ChaosMode.API;

public class Broadcaster
{
    public Broadcaster(ulong steamId)
    {
        SteamId = steamId;
        Player = null;
        ChaosEnabled = true;
    }

    public bool ChaosEnabled { get; set; }

    public BattleBitPlayer? Player { get; set; }

    public ulong SteamId { get; set; }
    
}