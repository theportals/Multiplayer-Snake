using System;
using System.Collections.Generic;
using Client.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Views;

public class GameplayView : GameStateView
{
    private ContentManager mContent;
    private GameModel mGameModel;
    private SpriteFont mFont;

    private bool connected = false; 
    private float timeout = 1;
    private float timer = 0;

    public override void initializeSession()
    {
        connected = false;
        timer = 0;
        // connect();
    }

    private void connect()
    {
        //TODO: Connect to server and actually update joined status
        mGameModel = new GameModel(mGame, mGraphics.PreferredBackBufferWidth, mGraphics.PreferredBackBufferHeight,
            mKeyboardInput, mMouseInput, mKeyboardInput.listenKeys);
        mGameModel.Initialize(mContent, mSpriteBatch);
    }

    public override void loadContent(ContentManager contentManager)
    {
        mContent = contentManager;
        mFont = mContent.Load<SpriteFont>("Fonts/menu");
    }

    public override void update(GameTime gameTime)
    {
        if (connected) mGameModel.update(gameTime);
        else
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer > timeout)
            {
                connected = true;
                // TODO: Actually connect
                connect();
            }
        }
    }

    public override void render(GameTime gameTime)
    {
        if (connected) mGameModel.render(gameTime);
        else
        {
            mSpriteBatch.Begin();
            
            DrawUtil.DrawStringsCentered(new List<string>
            {
                "Connecting...",
                $"Timeout in {Math.Ceiling(timeout - timer)}s"
            }, mFont, Color.White, mSpriteBatch);
            
            mSpriteBatch.End();
        }
    }
}