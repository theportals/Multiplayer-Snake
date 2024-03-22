using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Multiplayer_Snake.Input;

namespace Multiplayer_Snake.Views;

public interface GameState
{
    void initializeSession();
    void initialize(Client game, GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics, KeyboardInput keyboardInput, MouseInput mouseInput);
    void loadContent(ContentManager contentManager);
    void update(GameTime gameTime);
    void render(GameTime gameTime);
}