namespace ChaosMode.API;

public class Broadcaster
{
    public Broadcaster(ulong steamId)
    {
        SteamId = steamId;
        Player = null;
        ChaosEnabled = true;
        AcceptsRedeems = false;
        AcceptsVotes = true;
    }

    public bool AcceptsVotes { get; set; }

    public bool AcceptsRedeems { get; set; }

    public bool ChaosEnabled { get; set; }

    public BattleBitPlayer? Player { get; set; }

    public ulong SteamId { get; set; }
    
}