namespace Shared.Components;

public class PlayerInfo : Component
{
    public string playerName;
    public int score;
    public int kills;

    public PlayerInfo(string playerName, int score, int kills)
    {
        this.playerName = playerName;
        this.score = score;
        this.kills = kills;
    }
}