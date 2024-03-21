using Microsoft.Xna.Framework;

namespace Multiplayer_Snake.Input;

public interface InputDevice
{
    public delegate void CommandDelegate(GameTime gameTime);

    void Update(GameTime gameTime);
}