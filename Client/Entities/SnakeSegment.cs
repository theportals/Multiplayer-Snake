using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Multiplayer_Snake.Input;
using Shared.Entities;

namespace Multiplayer_Snake.Entities;

public class SnakeSegment
{
    private const float MOVE_SPEED = 100;
    private const float TURN_SPEED = 4;

    public static Entity create(Texture2D square, int x, int y)
    {
        return new Entity()
            .add(new Components.Appearance(square, Color.White, 10))
            .add(new Shared.Components.Position(x, y))
            .add(new Components.Collision(10f))
            .add(new Shared.Components.Movable(0f, MOVE_SPEED, TURN_SPEED))
            .add(new Components.Controllable());
    }
}