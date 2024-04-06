using Shared.Components;

namespace Shared.Entities;

public class SnakeSegment
{
    public const float MOVE_SPEED = 50;
    private const float TURN_SPEED = 4;

    public static Entity create(string spriteSheet, int x, int y, int segmentsToAdd=0, string playerName="NOT INITIALIZED")
    {
        var rng = new Random();
        return new Entity()
            .add(new Appearance(spriteSheet, 13, true, 3, 1024, 1024))
            .add(new Shared.Components.Position(x, y))
            .add(new Collision(10f))
            .add(new Shared.Components.Movable((float)(rng.NextDouble() * 2 * Math.PI), MOVE_SPEED, TURN_SPEED, segmentsToAdd))
            .add(new Controllable())
            .add(new Alive())
            .add(new Boostable(3, 1, 2, 0.5f, 0.75f))
            .add(new PlayerName(playerName))
            .add(new Snakeitude());
    }
}