using Microsoft.Xna.Framework.Input;
using Multiplayer_Snake.Input;

namespace Multiplayer_Snake.Components;

public class Controllable : Component
{
    public KeyboardInput mKeys;
    public MouseInput mMouse;
    public bool listenKeys;
    
    public Controllable(KeyboardInput keyboardInput, MouseInput mouseInput, bool listenKeys)
    {
        mKeys = keyboardInput;
        mMouse = mouseInput;
        this.listenKeys = listenKeys;
    }
}