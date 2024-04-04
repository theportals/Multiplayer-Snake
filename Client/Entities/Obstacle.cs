using System;
using Client.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Entities;

namespace Client.Entities;

public class Obstacle
{
    public static Entity create(string texture, int x, int y)
    {
        return new Entity()
            .add(new Components.Appearance(texture, 10, false, 0, 0, 0))
            .add(new RotationOffset((float)(-Math.PI / 2), 0f))
            .add(new Shared.Components.Position(x, y))
            .add(new Components.Collision(10f));
    }
}