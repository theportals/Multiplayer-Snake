using Shared.Components;

namespace Client.Components;

public class Collision : Component
{
    public float size;

    public Collision(float size)
    {
        this.size = size;
    }
}