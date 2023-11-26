using Newtonsoft.Json;

namespace ChaosMode.API;

public class RestEvent
{
    
    // "{"Referral": "String", "TwitchId": "Integer", "TwitchLogin": "String", "SteamId": "ulong", "EventType": "String"}"

    public RestEvent(string jsonString)
    {
        string[] entries = jsonString.Replace("{", "") // remove unneccesary chars and split
                                        .Replace("}", "")
                                        .Replace("\"", "")
                                        .Replace("\\", "")
                                        .Split(", ");
        foreach (var entry in entries)
        {
            string[] data = entry.Split(": ");
            switch (data[0])
            {
                case "Referral":
                    Referral = data[1];
                    break;
                case "TwitchId":
                    TwitchId = int.Parse(data[1]);
                    break;
                case "TwitchLogin":
                    TwitchLogin = data[1];
                    break;
                case "RedeemStr":
                    RedeemStr = data[1];
                    break;
                case "SteamId":
                    SteamId = ulong.Parse(data[1]);
                    break;
                case "EventType":
                    EventType = data[1];
                    break;
                case "Amount":
                    Amount = int.Parse(data[1]);
                    break;
                case "Tier":
                    Tier = int.Parse(data[1]);
                    break;
                case "Username":
                    Username = data[1];
                    break;
                case "TotalTime":
                    TotalTime = int.Parse(data[1]);
                    break;
                case "Streak":
                    Streak = int.Parse(data[1]);
                    break;
                case "GifterName":
                    GifterName = data[1];
                    break;
            }
        }
    }
    
    public string? Referral { get; set; }

    
    public int? TwitchId { get; set; }

    
    public string? TwitchLogin { get; set; }
    
    public string? RedeemStr { get; set; }

    public ulong SteamId { get; set; }

    public string EventType { get; set; }
    
    public int? Amount { get; set; }
    
    public int? Tier { get; set; }
    
    public string? Username { get; set; }
    
    public int? TotalTime { get; set; }
    
    public int? Streak { get; set; }
    
    public string? GifterName { get; set; }

    public override string ToString()
    {
        return $"EventType: {EventType}, Referral: {Referral}, TwitchId: {TwitchId}, TwitchLogin: {TwitchLogin}, RedeemStr: {RedeemStr}, SteamId: {SteamId}, Amount: {Amount}, Tier: {Tier}, Username: {Username}, TotalTime: {TotalTime}, Streak: {Streak}, GifterName: {GifterName}";
    }
    
}