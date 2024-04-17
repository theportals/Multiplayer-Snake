namespace Shared.Components;

public class Collision : Component
{
    public float size;
    public float intangibility; // Time left in seconds

    public Collision(float size, float intangibility=0f)
    {
        this.size = size;
        this.intangibility = intangibility;
    }
}