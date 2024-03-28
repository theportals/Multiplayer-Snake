using Microsoft.Xna.Framework;
using Shared.Components;

namespace Client.Components;

public class ColorOverride : Component
{
    public Color color;

    public ColorOverride(Color color)
    {
        this.color = color;
    }
}