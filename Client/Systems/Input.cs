using System;
using System.Collections.Generic;
using Client.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Shared.Components;
using Shared.Entities;

namespace Client.Systems;

public class Input : Shared.Systems.System
{
    private KeyboardInput mKeyboardInput;
    private MouseInput mMouseInput;
    private bool mListenKeys;

    private readonly int ARENA_SIZE;
    private readonly int WINDOW_WIDTH;
    private readonly int WINDOW_HEIGHT;
    private int OFFSET_X;
    private int OFFSET_Y;

    public float zoom;

    private int turn = 0;
    private int turnX = 0;
    private int turnY = 0;
    private bool mAbsCursor;

    private const float TURN_DEADZONE = (float)(2 * Math.PI / 180);

    private Dictionary<uint, uint> mClientToServerId = new();
    
    public Input(KeyboardInput keyboardInput, MouseInput mouseInput, bool listenKeys, int arenaSize, int windowWidth, int windowHeight, bool absCursor, float zoom=1)
        : base(typeof(Controllable))
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
            keyboardInput.registerCommand(InputDevice.Commands.UP, _ => turnY = -1, null, _ => turnY = 0);
            keyboardInput.registerCommand(InputDevice.Commands.DOWN, _ => turnY = 1, null, _ => turnY = 0);
            keyboardInput.registerCommand(InputDevice.Commands.LEFT, _ => turnX = -1, null, _ => turnX = 0);
            keyboardInput.registerCommand(InputDevice.Commands.RIGHT, _ => turnX = 1, null, _ => turnX = 0);
        }

        mAbsCursor = absCursor;
        this.zoom = zoom;
    }

    public void mapClientToServerId(uint clientId, uint serverId)
    {
        mClientToServerId[clientId] = serverId;
    }

    public void setAbsCursor(bool to)
    {
        mAbsCursor = to;
    }
    
    public override void update(TimeSpan gameTime)
    {
        mKeyboardInput.update(gameTime);
        mMouseInput.update(gameTime);
        
        foreach (var entry in mEntities)
        {
            var entity = entry.Value;
            var movable = entity.get<Shared.Components.Movable>();
            var pos = entity.get<Shared.Components.Position>();

            double facingNeeded;
            
            if (mListenKeys)
            {
                if (turnX != 0 || turnY != 0)
                {
                    facingNeeded = Math.Atan2(turnY, turnX);
                }
                else
                {
                    facingNeeded = movable.facing;
                }
            }
            else
            {
                var cpos = mMouseInput.getCursorPos();
                if (mAbsCursor)
                {
                    facingNeeded = Math.Atan2(cpos.Y - WINDOW_HEIGHT / 2, cpos.X - WINDOW_WIDTH / 2);
                }
                else
                {
                    OFFSET_X = (int)((WINDOW_WIDTH - ARENA_SIZE * zoom) / 2);
                    OFFSET_Y = (int)((WINDOW_HEIGHT - ARENA_SIZE * zoom) / 2);
                    var absX = pos.x * zoom + OFFSET_X;
                    var absY = pos.y * zoom + OFFSET_Y;
                    facingNeeded = Math.Atan2((cpos.Y - absY), (cpos.X - absX));
                }
            }
            
            var dl = movable.facing - facingNeeded;
            if (dl < 0) dl += 2 * Math.PI;
            var dr = facingNeeded - movable.facing;
            if (dr < 0) dr += 2 * Math.PI;

            if (Math.Min(dl, dr) <= TURN_DEADZONE) turn = 0;
            else if (dl < dr) turn = -1;
            else turn = 1;
            
            movable.facing += (float)(movable.turnSpeed * gameTime.TotalSeconds * turn);
            if (movable.facing < -Math.PI) movable.facing += (float)(2 * Math.PI);
            else if (movable.facing > Math.PI) movable.facing -= (float)(2 * Math.PI);

            var boost = entity.get<Boostable>();
            var id = mClientToServerId[entity.id];
            MessageQueueClient.instance.sendMessageWithId(new Shared.Messages.Input(id, movable.facing, boost.boosting, boost.stamina > 0, gameTime));
        }
    }
}