using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Multiplayer_Snake.Components;

public class Appearance : Component
{
    public Texture2D image;
    public Color color;

    public Appearance(Texture2D image, Color color)
    {
        this.image = image;
        this.color = color;
    }
}