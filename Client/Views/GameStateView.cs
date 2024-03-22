using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Multiplayer_Snake.Input;

namespace Multiplayer_Snake.Views;

public abstract class GameStateView : GameState
{
    protected Client mGame;
    protected GraphicsDeviceManager mGraphics;
    protected SpriteBatch mSpriteBatch;
    protected KeyboardInput mKeyboardInput;
    protected MouseInput mMouseInput;
    
    public virtual void initializeSession()
    {
        
    }

    public void initialize(Client game, GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics, KeyboardInput keyboardInput, MouseInput mouseInput)
    {
        mGame = game;
        mGraphics = graphics;
        mSpriteBatch = new SpriteBatch(graphicsDevice);
        mKeyboardInput = keyboardInput;
        mMouseInput = mouseInput;
    }

    public abstract void loadContent(ContentManager contentManager);

    public abstract void update(GameTime gameTime);

    public abstract void render(GameTime gameTime);
}