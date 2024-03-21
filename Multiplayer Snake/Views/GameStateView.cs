using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Multiplayer_Snake.Input;

namespace Multiplayer_Snake.Views;

public abstract class GameStateView : GameState
{
    protected MultiplayerSnakeGame mGame;
    protected GraphicsDeviceManager mGraphics;
    protected SpriteBatch mSpriteBatch;
    protected KeyboardInput mKeyboardInput;
    
    public virtual void initializeSession()
    {
        
    }

    public void initialize(MultiplayerSnakeGame game, GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics, KeyboardInput keyboardInput)
    {
        mGame = game;
        mGraphics = graphics;
        mSpriteBatch = new SpriteBatch(graphicsDevice);
        mKeyboardInput = keyboardInput;
    }

    public abstract void loadContent(ContentManager contentManager);

    public abstract void update(GameTime gameTime);

    public abstract void render(GameTime gameTime);
}