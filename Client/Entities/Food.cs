using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Entities;

namespace Multiplayer_Snake.Entities;

public class Food
{
    public static Entity create(Texture2D square, int x, int y)
    {
        return new Entity()
            .add(new Components.Appearance(square, Color.Red, 10))
            .add(new Shared.Components.Position(x, y))
            .add(new Components.Collision(10f))
            .add(new Components.Food());
    }
}