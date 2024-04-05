using System.Drawing;
using Shared.Components;

namespace Shared.Entities;

public class BorderBlock
{
    public static Entity create(string texture, int x, int y)
    {
        return new Entity()
            .add(new Appearance(texture, 10, false, 0, 0, 0))
            .add(new ColorOverride(Color.Black))
            .add(new Shared.Components.Position(x, y))
            .add(new Collision(10f));
    }
}