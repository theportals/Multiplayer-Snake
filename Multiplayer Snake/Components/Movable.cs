namespace Multiplayer_Snake.Components;

public class Movable : Component
{
    public float facing;
    public uint segmentsToAdd = 0;
    public float moveSpeed { get; private set; }
    public uint elapsedInterval = 0;

    public Movable(float facing, float moveSpeed)
    {
        this.facing = facing;
        this.moveSpeed = moveSpeed;
    }
}