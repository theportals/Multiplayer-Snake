using Shared.Components;

namespace Client.Components;

public class PlayerName : Component
{
    public string playerName;

    public PlayerName(string playerName)
    {
        this.playerName = playerName;
    }
}