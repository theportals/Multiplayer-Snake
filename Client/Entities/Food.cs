using System;
using Client.Components;
using Client.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Entities;

namespace Client.Entities;

public class Food
{
    public static Entity create(Texture2D square, int x, int y, bool naturalSpawn)
    {
        var rng = new ExtendedRandom();
        return new Entity()
            .add(new Appearance(square, 10))
            .add(new ColorOverride(Color.Red))
            .add(new Shared.Components.Position(x, y))
            .add(new Collision(10f))
            .add(new Components.Food(naturalSpawn))
            .add(new Lifetime((float)rng.nextGaussian(10, 2.5)));
    }
}