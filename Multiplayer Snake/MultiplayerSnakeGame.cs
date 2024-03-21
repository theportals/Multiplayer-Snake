using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Multiplayer_Snake.Views;

namespace Multiplayer_Snake;

public class MultiplayerSnakeGame : Game
{
    private GraphicsDeviceManager mGraphics;
    private SpriteBatch mSpriteBatch;
    private Dictionary<GameStates, GameState> mStates;
    private GameState mState;

    public MultiplayerSnakeGame()
    {
        mGraphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        mGraphics.IsFullScreen = false;
        mGraphics.PreferredBackBufferWidth = 1920;
        mGraphics.PreferredBackBufferHeight = 1080;
        mGraphics.ApplyChanges();

        mStates = new Dictionary<GameStates, GameState>
        {
            { GameStates.MAIN_MENU, new MainMenuView() },
            { GameStates.GAMEPLAY, new GameplayView() },
            { GameStates.HIGH_SCORES, new HighScoresView() },
            { GameStates.CONTROLS, new ControlsView() },
            { GameStates.CREDITS, new CreditsView() }
        };

        mState = mStates[GameStates.MAIN_MENU];

        base.Initialize();
    }

    protected override void LoadContent()
    {
        foreach (var state in mStates)
        {
            state.Value.initialize(GraphicsDevice, mGraphics);
            state.Value.loadContent(Content);
        }
    }

    protected override void Update(GameTime gameTime)
    {
        var nextState = mState.processInput(gameTime);

        if (nextState == GameStates.EXIT) Exit();
        
        // TODO: If things break, change mState after the draw step like in the class' ECSSnakeGame code
        if (mState != mStates[nextState])
        {
            mState = mStates[nextState];
            mState.initializeSession();
        }
        
        mState.update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        mState.render(gameTime);

        base.Draw(gameTime);
    }
}