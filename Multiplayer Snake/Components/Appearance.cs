using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Multiplayer_Snake.Components;

public class Appearance : Component
{
    public Texture2D image;
    public Color color;
    public int size;

    public Appearance(Texture2D image, Color color, int size)
    {
        this.image = image;
        this.color = color;
        this.size = size;
    }
}