namespace ChaosMode.Interfaces;

public interface IServerRoles
{
    string Admin { get; }
    string Moderator { get; }
    string Vip { get; }
    string Special { get; }
    string Default { get; }
}