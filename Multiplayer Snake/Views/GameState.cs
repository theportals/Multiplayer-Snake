using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Multiplayer_Snake.Input;

namespace Multiplayer_Snake.Views;

public interface GameState
{
    void initializeSession();
    void initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics, KeyboardInput keyboardInput);
    void loadContent(ContentManager contentManager);
    GameStates processInput(GameTime gameTime);
    void update(GameTime gameTime);
    void render(GameTime gameTime);
}