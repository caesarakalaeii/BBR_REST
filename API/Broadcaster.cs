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
        AcceptsFollows = false;
        AcceptsSubs = false;
        AcceptsCheers = false;
        AcceptsRaids = false;
    }

    public bool AcceptsRaids { get; set; }

    public bool AcceptsCheers { get; set; }

    public bool AcceptsSubs { get; set; }

    public bool AcceptsFollows { get; set; }

    public bool AcceptsVotes { get; set; }

    public bool AcceptsRedeems { get; set; }

    public bool ChaosEnabled { get; set; }

    public BattleBitPlayer? Player { get; set; }

    public ulong SteamId { get; set; }
    
}