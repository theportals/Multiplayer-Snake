using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Multiplayer_Snake.Input;

[DataContract(Name = "KeyBinds")]

public class KeyboardInput : InputDevice
{
    private Dictionary<InputDevice.Commands, InputDevice.CommandEntry> mCommandEntries = new();
    [DataMember] private Dictionary<Keys, KeyBind> mKeyBinds = new();
    private KeyboardState mPrevState;

    /// <summary>
    /// Registers a link between a command and a delegate to be called,
    /// such as registering Commands.UP to moving up in a menu.
    /// </summary>
    /// <param name="command">command</param>
    /// <param name="onPositiveEdge">Code executed when a key is first pressed</param>
    /// <param name="onHeld">Code executed every update after the first when key is held</param>
    /// <param name="onNegativeEdge">Code executed when a key is released</param>
    public void registerCommand(InputDevice.Commands command,
        InputDevice.CommandDelegate onPositiveEdge,
        InputDevice.CommandDelegate onHeld = null,
        InputDevice.CommandDelegate onNegativeEdge = null)
    {
        if (mCommandEntries.ContainsKey(command))
        {
            mCommandEntries.Remove(command);
        }
        mCommandEntries.Add(command, new InputDevice.CommandEntry(onPositiveEdge, onHeld, onNegativeEdge));
    }
    
    /// <summary>
    /// Registers a key to a command, such as W to Commands.UP
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="command">command</param>
    public void bindKey(Keys key, InputDevice.Commands command)
    {
        foreach (var conflictingBind in mKeyBinds.Where(bind => 
                     bind.Key.Equals(key)                       // Don't allow one key to perform multiple commands
                     || bind.Value.command.Equals(command)))    // Don't allow multiple keys to perform the same command
        {
            mKeyBinds.Remove(conflictingBind.Key);
        }
        
        mKeyBinds.Add(key, new KeyBind(key, command));
    }

    public Keys getKey(InputDevice.Commands command)
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
        mCommandEntries.Clear();
    }

    public void clearBinds()
    {
        mKeyBinds.Clear();
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

        if (!waitForEnd) endUpdate();
    }

    public void endUpdate()
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

    [DataContract(Name = "KeyBind")]
    private struct KeyBind
    {
        public KeyBind(Keys key, InputDevice.Commands command)
        {
            this.key = key;
            this.command = command;
        }

        [DataMember] public Keys key;
        [DataMember] public InputDevice.Commands command;
    }
}