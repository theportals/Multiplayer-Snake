using Shared.Components;

namespace Shared.Components;

public class Movable : Component
{
    public float facing;
    public uint segmentsToAdd = 0;
    public float moveSpeed { get; private set; }
    public float turnSpeed { get; private set; }

    public Movable(float facing, float moveSpeed, float turnSpeed)
    {
        this.facing = facing;
        this.moveSpeed = moveSpeed;
        this.turnSpeed = turnSpeed;
    }
}