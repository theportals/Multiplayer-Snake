using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Multiplayer_Snake.Input;

namespace Multiplayer_Snake.Systems;

public class Input : Shared.Systems.System
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
    private bool mAbsCursor;

    private const float TURN_DEADZONE = (float)(2 * Math.PI / 180);
    
    public Input(KeyboardInput keyboardInput, MouseInput mouseInput, bool listenKeys, int arenaSize, int windowWidth, int windowHeight, bool absCursor)
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

        mAbsCursor = absCursor;
    }

    public void setAbsCursor(bool to)
    {
        mAbsCursor = to;
    }
    
    public override void Update(TimeSpan gameTime)
    {
        mKeyboardInput.update(gameTime);
        mMouseInput.update(gameTime);
        
        foreach (var entity in mEntities.Values)
        {
            var movable = entity.get<Shared.Components.Movable>();
            var pos = entity.get<Shared.Components.Position>();
            
            if (!mListenKeys)
            {
                double angleToCursor = 0;
                var cpos = mMouseInput.getCursorPos();
                if (mAbsCursor)
                {
                    angleToCursor = Math.Atan2(cpos.Y - WINDOW_HEIGHT / 2, cpos.X - WINDOW_WIDTH / 2);
                }
                else
                {
                    var absX = pos.x + OFFSET_X;
                    var absY = pos.y + OFFSET_Y;
                    angleToCursor = Math.Atan2((cpos.Y - absY), (cpos.X - absX));
                }

                var dl = movable.facing - angleToCursor;
                if (dl < 0) dl += 2 * Math.PI;
                var dr = angleToCursor - movable.facing;
                if (dr < 0) dr += 2 * Math.PI;

                if (Math.Min(dl, dr) <= TURN_DEADZONE) turn = 0;
                else if (dl < dr) turn = -1;
                else turn = 1;
            }
            movable.facing += (float)(movable.turnSpeed * gameTime.TotalSeconds * turn);
            if (movable.facing < -Math.PI) movable.facing += (float)(2 * Math.PI);
            else if (movable.facing > Math.PI) movable.facing -= (float)(2 * Math.PI);
        }
    }
}