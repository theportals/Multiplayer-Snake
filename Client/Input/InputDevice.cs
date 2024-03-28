using System;
using Microsoft.Xna.Framework;

namespace Client.Input;

public interface InputDevice
{
    public delegate void CommandDelegate(TimeSpan gameTime);

    public void update(TimeSpan gameTime, bool waitForEnd = false);
    public void endUpdate();

    public enum Commands
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        SELECT,
        BACK,
        BOOST
    }
    
    protected struct CommandEntry
    {
        public CommandEntry(CommandDelegate onPosEdge, CommandDelegate onHeld, CommandDelegate onNegEdge)
        {
            this.onPosEdge = onPosEdge;
            this.onHeld = onHeld;
            this.onNegEdge = onNegEdge;
        }

        public CommandDelegate onPosEdge;   // Positive edge; called once when first active
        public CommandDelegate onHeld;      // Held; called every update after the first active update
        public CommandDelegate onNegEdge;   // Negative edge; called once when no longer active
    }
}