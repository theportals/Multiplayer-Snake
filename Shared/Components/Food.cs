namespace Shared.Components;

public class Food : Component
{
    public bool naturalSpawn;

    public Food(bool naturalSpawn)
    {
        this.naturalSpawn = naturalSpawn;
    }
}