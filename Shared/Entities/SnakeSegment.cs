using Shared.Entities;

namespace Client.Entities;

public class SnakeSegment
{
    public const float MOVE_SPEED = 100;
    private const float TURN_SPEED = 4;

    public static Entity create(string spriteSheet, int x, int y)
    {
        var rng = new Random();
        return new Entity()
            .add(new Components.Appearance(spriteSheet, 13, true, 3, 1024, 1024))
            .add(new Shared.Components.Position(x, y))
            .add(new Components.Collision(10f))
            .add(new Shared.Components.Movable((float)(rng.NextDouble() * 2 * Math.PI), MOVE_SPEED, TURN_SPEED))
            .add(new Components.Controllable())
            .add(new Components.Alive())
            .add(new Components.Boostable(3, 1, 2, 0.5f, 0.75f))
            .add(new Components.PlayerName("NOT INITIALIZED"))
            .add(new Components.Snakeitude());
    }
}