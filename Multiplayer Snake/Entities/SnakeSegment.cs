using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Multiplayer_Snake.Input;

namespace Multiplayer_Snake.Entities;

public class SnakeSegment
{
    private const float MOVE_SPEED = 100;
    private const float TURN_SPEED = 4;

    public static Entity create(Texture2D square, int x, int y)
    {
        return new Entity()
            .Add(new Components.Appearance(square, Color.White, 10))
            .Add(new Components.Position(x, y))
            .Add(new Components.Collision(10f))
            .Add(new Components.Movable(0f, MOVE_SPEED, TURN_SPEED))
            .Add(new Components.Controllable());
    }
}