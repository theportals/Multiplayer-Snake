using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Multiplayer_Snake.Input;
using Multiplayer_Snake.Util;
using Multiplayer_Snake.Views;
using Multiplayer_Snake.Views.Menus;

namespace Multiplayer_Snake;

public class Client : Game
{
    private GraphicsDeviceManager mGraphics;
    private SpriteBatch mSpriteBatch;
    private Dictionary<GameStates, GameState> mStates;
    private GameState mState;
    private KeyboardInput mKeyboardInput;
    private MouseInput mMouseInput;

    public Client()
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

        mMouseInput = new MouseInput();

        mKeyboardInput = (KeyboardInput)StorageUtil.loadData<KeyboardInput>("keybinds.json");
        if (mKeyboardInput == null)
        {
            mKeyboardInput = new KeyboardInput();
            mKeyboardInput.bindKey(Keys.Up, InputDevice.Commands.UP);
            mKeyboardInput.bindKey(Keys.Down, InputDevice.Commands.DOWN);
            mKeyboardInput.bindKey(Keys.Left, InputDevice.Commands.LEFT);
            mKeyboardInput.bindKey(Keys.Right, InputDevice.Commands.RIGHT);
            mKeyboardInput.bindKey(Keys.Z, InputDevice.Commands.SELECT);
            mKeyboardInput.bindKey(Keys.X, InputDevice.Commands.BACK);
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        foreach (var state in mStates)
        {
            state.Value.initialize(this, GraphicsDevice, mGraphics, mKeyboardInput, mMouseInput);
            state.Value.loadContent(Content);
        }
        mState.initializeSession();
    }

    public void changeState(GameStates nextState)
    {
        if (nextState == GameStates.EXIT) Exit();
        mState = mStates[nextState];
        mState.initializeSession();
    }

    protected override void Update(GameTime gameTime)
    {
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