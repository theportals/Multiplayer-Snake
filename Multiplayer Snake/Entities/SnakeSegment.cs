using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Multiplayer_Snake.Input;

namespace Multiplayer_Snake.Entities;

public class SnakeSegment
{
    private const int MOVE_SPEED = 10;

    public static Entity create(Texture2D square, KeyboardInput keys, MouseInput mouse, bool listenKeys, int x, int y)
    {
        return new Entity()
            .Add(new Components.Appearance(square, Color.White))
            .Add(new Components.Position(x, y))
            .Add(new Components.Collision(5f))
            .Add(new Components.Movable(0f, MOVE_SPEED))
            .Add(new Components.Controllable(keys, mouse, listenKeys));
    }
}