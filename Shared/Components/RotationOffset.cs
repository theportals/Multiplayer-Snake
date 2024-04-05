namespace Shared.Components;

public class RotationOffset : Component
{
    public float offset;
    public float rotationSpeed;

    public RotationOffset(float offset, float rotationSpeed)
    {
        this.offset = offset;
        this.rotationSpeed = rotationSpeed;
    }
}