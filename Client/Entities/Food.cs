using System;
using Client.Components;
using Client.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Entities;

namespace Client.Entities;

public class Food
{
    public static Entity create(string texture, int x, int y, bool naturalSpawn)
    {
        var rng = new ExtendedRandom();
        var size = rng.Next(10) + 7.5f;
        return new Entity()
            .add(new Appearance(texture, (int)size, true, 4, 1024, 1024, rng.Next(4)))
            .add(new RotationOffset((float)(-Math.PI / 2), 0f))
            .add(new Shared.Components.Position(x, y))
            .add(new Collision(size))
            .add(new Components.Food(naturalSpawn))
            .add(new Lifetime((float)rng.nextGaussian(10, 2.5)));
    }
}