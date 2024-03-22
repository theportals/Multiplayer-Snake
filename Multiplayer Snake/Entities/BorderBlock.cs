using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Multiplayer_Snake.Entities;

public class BorderBlock
{
    public static Entity create(Texture2D square, int x, int y)
    {
        return new Entity()
            .Add(new Components.Appearance(square, Color.Black, 10))
            .Add(new Components.Position(x, y))
            .Add(new Components.Collision(10f));
    }
}