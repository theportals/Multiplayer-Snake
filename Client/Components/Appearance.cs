using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;

namespace Client.Components;

public class Appearance : Component
{
    public Texture2D image;
    public int size;

    public Appearance(Texture2D image, int size)
    {
        this.image = image;
        this.size = size;
    }
}