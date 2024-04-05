using Shared.Components;

namespace Shared.Components;

public class Movable : Component
{
    public float facing;
    public int segmentsToAdd = 0;
    public float moveSpeed { get; private set; }
    public float turnSpeed { get; private set; }
    public bool boosting = false;

    public Movable(float facing, float moveSpeed, float turnSpeed, int segmentsToAdd=0)
    {
        this.facing = facing;
        this.moveSpeed = moveSpeed;
        this.turnSpeed = turnSpeed;
        this.segmentsToAdd = segmentsToAdd;
    }
}