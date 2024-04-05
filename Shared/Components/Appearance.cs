namespace Shared.Components;

public class Appearance : Component
{
    public string texture;
    public int displaySize;
    public bool animated;
    public int frames;
    public int frameWidth;
    public int frameHeight;
    public int? staticFrame;

    public Appearance(string texture, int displaySize, bool animated, int frames, int frameWidth, int frameHeight, int? staticFrame=null)
    {
        this.texture = texture;
        this.displaySize = displaySize;
        this.animated = animated;
        this.frames = frames;
        this.frameWidth = frameWidth;
        this.frameHeight = frameHeight;
        this.staticFrame = staticFrame;
    }
}