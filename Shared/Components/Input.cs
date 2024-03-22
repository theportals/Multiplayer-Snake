namespace Shared.Components;

public class Input : Component
{
    public List<Type> inputs { get; private set; }
    
    public enum Type : UInt16
    {
        MOVE,
        TURN_LEFT,
        TURN_RIGHT
    }

    public Input(List<Type> inputs)
    {
        this.inputs = inputs;
    }
}