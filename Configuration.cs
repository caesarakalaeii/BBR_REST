using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BattleBitAPI.Common;


namespace ChaosMode;

public class Configuration
{
    internal class ServerConfiguration
    {
        [JsonIgnore] public IPAddress? IPAddress { get; set; }
        [Required] public string IP { get; set; } = "0.0.0.0";
        [Required] public int Port { get; set; } = 30001;
        [Required] public string LoadingScreenText { get; set; } = "TTV 2 BBR";
        [Required] public LogLevel LogLevel { get; set; } = LogLevel.Players | LogLevel.GameServers | LogLevel.GameServerErrors | LogLevel.Sockets;

        [Required] public int RestPort { get; set; } = 5001;

        [Required] public Dictionary<string, string> PlayerChatPrefixes { get; set; } = new()
        {
            { Program.BattleBitServerRoles.Admin, "<color=#05C3DD>" },
            { Program.BattleBitServerRoles.Moderator, "<color=#05C3DD>" },
            { Program.BattleBitServerRoles.Vip, "<color=#05C3DD>" },
            { Program.BattleBitServerRoles.Special, "<color=#05C3DD>" },
            { Program.BattleBitServerRoles.Default, "<color=#05C3DD>" },
        };

        [Required] public Dictionary<string, string> PlayerChatSuffixes { get; set; } = new()
        {
            { Program.BattleBitServerRoles.Admin, "</color> <color=#FF0000>[Server Admin]</color> <sprite index=8><sprite index=7><sprite index=3>" },
            { Program.BattleBitServerRoles.Moderator, "</color> <color=purple>[Server Mod]</color> <sprite index=0>" },
            { Program.BattleBitServerRoles.Vip, "</color> <color=yellow>[VIP]</color> <sprite index=6>" },
            { Program.BattleBitServerRoles.Special, "</color> <color=green>[Special]</color> <sprite index=4>" },
            { Program.BattleBitServerRoles.Default, "</color>" }
        };
    }
}