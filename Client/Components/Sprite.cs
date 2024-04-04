using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;

namespace Client.Components;

public class Sprite : Component
{
    public Texture2D texture { get; private set; }
    public Vector2 center { get; private set; }
    
    public Sprite(Texture2D texture)
    {
        this.texture = texture;
        center = new Vector2(texture.Width / 2, texture.Height / 2);
    }
}