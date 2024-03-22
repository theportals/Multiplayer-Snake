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

    private readonly int ARENA_SIZE;
    private readonly int WINDOW_WIDTH;
    private readonly int WINDOW_HEIGHT;
    private readonly int OFFSET_X;
    private readonly int OFFSET_Y;

    private int turn = 0;
    
    public Input(KeyboardInput keyboardInput, MouseInput mouseInput, bool listenKeys, int arenaSize, int windowWidth, int windowHeight)
        : base(typeof(Components.Controllable))
    {
        mKeyboardInput = keyboardInput;
        mMouseInput = mouseInput;
        mListenKeys = listenKeys;

        ARENA_SIZE = arenaSize;
        WINDOW_WIDTH = windowWidth;
        WINDOW_HEIGHT = windowHeight;
        OFFSET_X = (windowWidth - arenaSize) / 2;
        OFFSET_Y = (windowHeight - arenaSize) / 2;
        
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
            var pos = entity.GetComponent<Components.Position>();
            
            if (!mListenKeys)
            {
                var cpos = mMouseInput.getCursorPos();
                var absX = pos.x + OFFSET_X;
                var absY = pos.y + OFFSET_Y;
                var angleToCursor = Math.Atan2((cpos.Y - absY), (cpos.X - absX));

                var dl = movable.facing - angleToCursor;
                if (dl < 0) dl += 2 * Math.PI;
                var dr = angleToCursor - movable.facing;
                if (dr < 0) dr += 2 * Math.PI;

                if (dl < dr) turn = -1;
                else turn = 1;
            }
            movable.facing += (float)(movable.turnSpeed * gameTime.ElapsedGameTime.TotalSeconds * turn);
            if (movable.facing < -Math.PI) movable.facing += (float)(2 * Math.PI);
            else if (movable.facing > Math.PI) movable.facing -= (float)(2 * Math.PI);
        }
    }
}