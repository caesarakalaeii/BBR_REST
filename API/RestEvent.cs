using Newtonsoft.Json;

namespace ChaosMode.API;

public class RestEvent
{
    [JsonProperty("Referral")]
    public string? Referral { get; set; }

    [JsonProperty("TwitchId")]
    public int? TwitchId { get; set; }

    [JsonProperty("TwitchLogin")]
    public string? TwitchLogin { get; set; }
    
    [JsonProperty("RedeemStr")]
    public string? RedeemStr { get; set; }

    [JsonProperty("SteamId")]
    public ulong SteamId { get; set; }

    [JsonProperty("EventType")]
    public string EventType { get; set; }
    
    [JsonProperty("Amount")]
    public int? Amount { get; set; }
    [JsonProperty("Tier")]
    public int? Tier { get; set; }

}