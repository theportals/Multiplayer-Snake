using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;

namespace Client.Components;

public class Appearance : Component
{
    public Texture2D image;
    public int displaySize;
    public bool animated;
    public int frames;
    public int frameWidth;
    public int frameHeight;
    public int? staticFrame;

    public Appearance(Texture2D image, int displaySize, bool animated, int frames, int frameWidth, int frameHeight, int? staticFrame=null)
    {
        this.image = image;
        this.displaySize = displaySize;
        this.animated = animated;
        this.frames = frames;
        this.frameWidth = frameWidth;
        this.frameHeight = frameHeight;
        this.staticFrame = staticFrame;
    }
}