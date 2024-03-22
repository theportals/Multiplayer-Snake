using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Entities;

namespace Multiplayer_Snake.Entities;

public class Obstacle
{
    public static Entity create(Texture2D square, int x, int y)
    {
        return new Entity()
            .add(new Components.Appearance(square, Color.Green, 10))
            .add(new Shared.Components.Position(x, y))
            .add(new Components.Collision(10f));
    }
}