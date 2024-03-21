using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Multiplayer_Snake.Views;

public abstract class GameStateView : GameState
{
    protected GraphicsDeviceManager mGraphics;
    protected SpriteBatch mSpriteBatch;
    
    public virtual void initializeSession()
    {
        
    }

    public void initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics)
    {
        mGraphics = graphics;
        mSpriteBatch = new SpriteBatch(graphicsDevice);
    }

    public abstract void loadContent(ContentManager contentManager);

    public abstract GameStates processInput(GameTime gameTime);

    public abstract void update(GameTime gameTime);

    public abstract void render(GameTime gameTime);
}