using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Multiplayer_Snake.Input;
using Multiplayer_Snake.Util;
using Multiplayer_Snake.Views;

namespace Multiplayer_Snake;

public class MultiplayerSnakeGame : Game
{
    private GraphicsDeviceManager mGraphics;
    private SpriteBatch mSpriteBatch;
    private Dictionary<GameStates, GameState> mStates;
    private GameState mState;
    private KeyboardInput mKeyboardInput;

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

        mKeyboardInput = (KeyboardInput)StorageUtil.loadData<KeyboardInput>("keybinds.json");
        if (mKeyboardInput == null)
        {
            mKeyboardInput = new KeyboardInput();
            mKeyboardInput.bindKey(Keys.Up, KeyboardInput.Commands.UP);
            mKeyboardInput.bindKey(Keys.Down, KeyboardInput.Commands.DOWN);
            mKeyboardInput.bindKey(Keys.Left, KeyboardInput.Commands.LEFT);
            mKeyboardInput.bindKey(Keys.Right, KeyboardInput.Commands.RIGHT);
            mKeyboardInput.bindKey(Keys.Z, KeyboardInput.Commands.SELECT);
            mKeyboardInput.bindKey(Keys.X, KeyboardInput.Commands.BACK);
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        foreach (var state in mStates)
        {
            state.Value.initialize(GraphicsDevice, mGraphics, mKeyboardInput);
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