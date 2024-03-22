using Shared.Components;

namespace Multiplayer_Snake.Components;

public class Collision : Component
{
    public float size;

    public Collision(float size)
    {
        this.size = size;
    }
}