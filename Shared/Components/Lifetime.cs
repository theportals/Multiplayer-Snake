namespace Shared.Components;

public class Lifetime : Component
{
    public float lifetime;
    public float timeAlive;

    public Lifetime(float lifetime)
    {
        this.lifetime = lifetime;
    }
}