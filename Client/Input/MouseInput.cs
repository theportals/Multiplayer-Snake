using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Client.Input;

public class MouseInput : InputDevice
{
    private Dictionary<MouseRegion, InputDevice.CommandEntry> mMouseRegions = new();
    private MouseState mPrevState;

    public enum MouseActions
    {
        HOVER,
        L_CLICK,
        R_CLICK,
        M_CLICK,
        SCROLL_UP,
        SCROLL_DOWN
    }

    public void registerMouseRegion(Rectangle? rectangle,
        MouseActions action,
        InputDevice.CommandDelegate onPositiveEdge = null,
        InputDevice.CommandDelegate onHeld = null,
        InputDevice.CommandDelegate onNegativeEdge = null,
        bool requireCursorInRegion = true)
    {
        var mr = new MouseRegion(rectangle, action, requireCursorInRegion);
        if (mMouseRegions.ContainsKey(mr)) mMouseRegions.Remove(mr);
        mMouseRegions.Add(mr, new InputDevice.CommandEntry(onPositiveEdge, onHeld, onNegativeEdge));
    }

    public void clearRegions()
    {
        mMouseRegions.Clear();
    }

    public Vector2 getCursorPos()
    {
        var state = Mouse.GetState();
        return new Vector2(state.X, state.Y);
    }
    
    public void update(TimeSpan gameTime, bool waitForEnd = false)
    {
        for (int i = 0; i < mMouseRegions.Count; i++)
        {
            var region = mMouseRegions.ElementAt(i).Key;
            var entry = mMouseRegions.ElementAt(i).Value;

            if (positiveEdge(region.region, region.action, region.requireCursor))
            {
                entry.onPosEdge?.Invoke(gameTime);
            } else if (held(region.region, region.action))
            {
                entry.onHeld?.Invoke(gameTime);
            } else if (negativeEdge(region.region, region.action, region.requireCursor))
            {
                entry.onNegEdge?.Invoke(gameTime);
            }
        }
        
        if (!waitForEnd) endUpdate();
    }

    public void endUpdate()
    {
        mPrevState = Mouse.GetState();
    }

    private bool positiveEdge(Rectangle? region, MouseActions action, bool requireCursor)
    {
        var state = Mouse.GetState();
        if (requireCursor)
        {
            if (!cursorInRectangle(state, region)) return false;
        }
        return action switch
        {
            MouseActions.HOVER => !cursorInRectangle(mPrevState, region),
            MouseActions.L_CLICK => state.LeftButton == ButtonState.Pressed &&
                                    mPrevState.LeftButton == ButtonState.Released,
            MouseActions.R_CLICK => state.RightButton == ButtonState.Pressed &&
                                    mPrevState.RightButton == ButtonState.Released,
            MouseActions.M_CLICK => state.MiddleButton == ButtonState.Pressed &&
                                    mPrevState.MiddleButton == ButtonState.Released,
            MouseActions.SCROLL_UP => state.ScrollWheelValue > mPrevState.ScrollWheelValue,
            MouseActions.SCROLL_DOWN => state.ScrollWheelValue < mPrevState.ScrollWheelValue,
            _ => false
        };
    }

    private bool held(Rectangle? region, MouseActions action)
    {
        var state = Mouse.GetState();
        var inRegion = cursorInRectangle(state, region);
        if (!inRegion) return false;
        return action switch
        {
            MouseActions.HOVER => true,
            MouseActions.L_CLICK => state.LeftButton == ButtonState.Pressed,
            MouseActions.R_CLICK => state.RightButton == ButtonState.Pressed,
            MouseActions.M_CLICK => state.MiddleButton == ButtonState.Pressed,
            MouseActions.SCROLL_UP or MouseActions.SCROLL_DOWN => false,
            _ => false
        };
    }

    private bool negativeEdge(Rectangle? region, MouseActions action, bool requireCursor)
    {
        var state = Mouse.GetState();
        var hasCursor = true;
        if (requireCursor)
        {
            hasCursor = cursorInRectangle(state, region);
        }

        return action switch
        {
            MouseActions.HOVER => !hasCursor && cursorInRectangle(mPrevState, region),
            MouseActions.L_CLICK => hasCursor && state.LeftButton == ButtonState.Released &&
                                    mPrevState.LeftButton == ButtonState.Pressed,
            MouseActions.R_CLICK => hasCursor && state.RightButton == ButtonState.Released &&
                                    mPrevState.RightButton == ButtonState.Pressed,
            MouseActions.M_CLICK => hasCursor && state.MiddleButton == ButtonState.Released &&
                                    mPrevState.MiddleButton == ButtonState.Pressed,
            MouseActions.SCROLL_UP or MouseActions.SCROLL_DOWN => false,
            _ => false
        };
    }

    private bool cursorInRectangle(MouseState state, Rectangle? r)
    {
        if (r == null) return true;
        return state.X > r.Value.Left && state.X < r.Value.Right && state.Y > r.Value.Top && state.Y < r.Value.Bottom;
    }

    private struct MouseRegion
    {
        public MouseRegion(Rectangle? region, MouseActions action, bool requireCursor)
        {
            this.region = region;
            this.action = action;
            this.requireCursor = requireCursor;
        }        
        
        public Rectangle? region;
        public MouseActions action;
        public bool requireCursor;
    }
}