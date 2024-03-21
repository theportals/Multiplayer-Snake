using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Multiplayer_Snake.Input;

[DataContract(Name = "KeyBinds")]

public class KeyboardInput
{
    private Dictionary<Commands, CommandEntry> mCommandEntries = new();
    [DataMember] private Dictionary<Keys, KeyBind> mKeyBinds = new();
    private KeyboardState mPrevState;

    public enum Commands
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        SELECT,
        BACK
    }

    /// <summary>
    /// Registers a link between a command and a delegate to be called,
    /// such as registering Commands.UP to moving up in a menu.
    /// </summary>
    /// <param name="command">command</param>
    /// <param name="onPositiveEdge">Code executed when a key is first pressed</param>
    /// <param name="onHeld">Code executed every update after the first when key is held</param>
    /// <param name="onNegativeEdge">Code executed when a key is released</param>
    public void registerCommand(Commands command,
        InputDevice.CommandDelegate onPositiveEdge,
        InputDevice.CommandDelegate onHeld = null,
        InputDevice.CommandDelegate onNegativeEdge = null)
    {
        if (mCommandEntries.ContainsKey(command))
        {
            mCommandEntries.Remove(command);
        }
        mCommandEntries.Add(command, new CommandEntry(command, onPositiveEdge, onHeld, onNegativeEdge));
    }
    
    /// <summary>
    /// Registers a key to a command, such as W to Commands.UP
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="command">command</param>
    public void bindKey(Keys key, Commands command)
    {
        foreach (var conflictingBind in mKeyBinds.Where(bind => 
                     bind.Key.Equals(key)                       // Don't allow one key to perform multiple commands
                     || bind.Value.command.Equals(command)))    // Don't allow multiple keys to perform the same command
        {
            mKeyBinds.Remove(conflictingBind.Key);
        }
        
        mKeyBinds.Add(key, new KeyBind(key, command));
    }

    public Keys getKey(Commands command)
    {
        return mKeyBinds.FirstOrDefault(c => c.Value.command.Equals(command)).Key;
    }

    public List<Keys> getNewPositiveEdges()
    {
        return Enum.GetValues<Keys>().Where(positiveEdge).ToList();
    }

    public void clearKey(Keys key)
    {
        mKeyBinds.Remove(key);
    }

    public void clearCommands()
    {
        mCommandEntries = new Dictionary<Commands, CommandEntry>();
    }

    public void clearBinds()
    {
        mKeyBinds = new Dictionary<Keys, KeyBind>();
    }

    public void update(GameTime gameTime, bool waitForEnd = false)
    {
        KeyboardState state = Keyboard.GetState();
        for (int i = 0; i < mKeyBinds.Count; i++)
        {
            var entry = mKeyBinds.ElementAt(i).Value;
            Keys key = entry.key;
            if (!mCommandEntries.ContainsKey(entry.command)) continue;

            if (positiveEdge(key))
            {
                mCommandEntries[entry.command].onPosEdge?.Invoke(gameTime);
            } else if (state.IsKeyDown(key))
            {
                mCommandEntries[entry.command].onHeld?.Invoke(gameTime);
            } else if (negativeEdge(key))
            {
                mCommandEntries[entry.command].onNegEdge?.Invoke(gameTime);
            }
        }

        if (!waitForEnd) EndUpdate();
    }

    public void EndUpdate()
    {
        mPrevState = Keyboard.GetState();
    }

    private bool positiveEdge(Keys key)
    {
        return Keyboard.GetState().IsKeyDown(key) && !mPrevState.IsKeyDown(key);
    }

    private bool negativeEdge(Keys key)
    {
        return !Keyboard.GetState().IsKeyDown(key) && mPrevState.IsKeyDown(key);
    }
    
    private struct CommandEntry
    {
        public CommandEntry(Commands command,
            InputDevice.CommandDelegate onPosEdge, InputDevice.CommandDelegate onHeld, InputDevice.CommandDelegate onNegEdge)
        {
            this.command = command;
            this.onPosEdge = onPosEdge;
            this.onHeld = onHeld;
            this.onNegEdge = onNegEdge;
        }

        public Commands command;
        public InputDevice.CommandDelegate onPosEdge;   // Positive edge; called once when key is first pressed
        public InputDevice.CommandDelegate onHeld;      // Held; called every update after the first when key is held
        public InputDevice.CommandDelegate onNegEdge;   // Negative edge; called once when key is released
    }

    [DataContract(Name = "KeyBind")]
    private struct KeyBind
    {
        public KeyBind(Keys key, Commands command)
        {
            this.key = key;
            this.command = command;
        }

        [DataMember] public Keys key;
        [DataMember] public Commands command;
    }
}