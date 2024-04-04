using System;
using System.Collections.Generic;
using Client.Input;
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
    private bool firstFrame = true;
    private float timeout = 5;
    private float timer = 0;

    public override void initializeSession()
    {
        timer = 0;
        firstFrame = true;
        connected = false;
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
        if (firstFrame)
        {
            render(gameTime);
            firstFrame = false;
            connected = MessageQueueClient.instance.initialize("localhost", 3000);
            return;
        }
        if (connected) mGameModel.update(gameTime);
        else
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer >= timeout) mGame.changeState(GameStates.MAIN_MENU);
        };
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