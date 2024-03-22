using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Multiplayer_Snake.Views;

public class GameplayView : GameStateView
{
    private ContentManager mContent;
    private GameModel mGameModel;

    public override void initializeSession()
    {
        mGameModel = new GameModel(mGame, mGraphics.PreferredBackBufferWidth, mGraphics.PreferredBackBufferHeight,
            mKeyboardInput, mMouseInput, false);
        mGameModel.Initialize(mContent, mSpriteBatch);
    }

    public override void loadContent(ContentManager contentManager)
    {
        mContent = contentManager;
    }

    public override void update(GameTime gameTime)
    {
        mGameModel.update(gameTime);
    }

    public override void render(GameTime gameTime)
    {
        mGameModel.Draw(gameTime);
    }
}