using ChaosMode.Enums;
using ChaosMode.Interfaces;

namespace ChaosMode.API;

public class ServerRoles : IServerRoles
{
    public string Admin => PlayerRoles.Admin.ToString();
    public string Moderator => PlayerRoles.Moderator.ToString();
    public string Vip => PlayerRoles.Vip.ToString();
    public string Special => PlayerRoles.Special.ToString();
    public string Default => PlayerRoles.Default.ToString();
}