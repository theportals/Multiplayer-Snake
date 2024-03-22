using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Multiplayer_Snake.Input;

namespace Multiplayer_Snake.Systems;

public class Input : System
{
    private KeyboardInput mKeyboardInput;
    private MouseInput mMouseInput;
    private bool mListenKeys;

    private int turn = 0;
    
    public Input(KeyboardInput keyboardInput, MouseInput mouseInput, bool listenKeys)
        : base(typeof(Components.Controllable))
    {
        mKeyboardInput = keyboardInput;
        mMouseInput = mouseInput;
        mListenKeys = listenKeys;
        
        if (listenKeys)
        {
            keyboardInput.registerCommand(InputDevice.Commands.LEFT, _ => turn = -1, null, _ => turn = 0);
            keyboardInput.registerCommand(InputDevice.Commands.RIGHT, _ => turn = 1, null, _ => turn = 0);
        }
    }
    
    public override void Update(GameTime gameTime)
    {
        mKeyboardInput.update(gameTime);
        mMouseInput.update(gameTime);
        
        foreach (var entity in mEntities.Values)
        {
            var movable = entity.GetComponent<Components.Movable>();
            // var pos = entity.GetComponent<Components.Position>();
            // var input = entity.GetComponent<Components.Controllable>();
            
            if (!mListenKeys)
            {
                var cpos = mMouseInput.getCursorPos();
            }
            
            movable.facing += (float)(movable.turnSpeed * gameTime.ElapsedGameTime.TotalSeconds * turn);
        }
    }
}