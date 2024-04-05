using System.Drawing;

namespace Shared.Components;

public class ColorOverride : Component
{
    public Color color;

    public ColorOverride(Color color)
    {
        this.color = color;
    }
}