using System;
using System.Collections.Generic;
using Client.Input;
using Client.Util;
using Client.Views;
using Client.Views.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;

namespace Client;

public class Client : Game
{
    private GraphicsDeviceManager mGraphics;
    private SpriteBatch mSpriteBatch;
    private Dictionary<GameStates, GameState> mStates;
    private GameState mState;
    private KeyboardInput mKeyboardInput;
    private MouseInput mMouseInput;
    public List<Tuple<int, DateTime>> mHighscores;
    public static Texture2D pixel;

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
        
        pixel = new Texture2D(mGraphics.GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

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

        mHighscores = (List<Tuple<int, DateTime>>)StorageUtil.loadData<List<Tuple<int, DateTime>>>("highscores.json");
        if (mHighscores == null)
        {
            mHighscores = new List<Tuple<int, DateTime>>();
        }
        mKeyboardInput = (KeyboardInput)StorageUtil.loadData<KeyboardInput>("keybinds.json");
        if (mKeyboardInput == null)
        {
            mKeyboardInput = new KeyboardInput();
            mKeyboardInput.bindKey(Keys.Up, InputDevice.Commands.UP);
            mKeyboardInput.bindKey(Keys.Down, InputDevice.Commands.DOWN);
            mKeyboardInput.bindKey(Keys.Left, InputDevice.Commands.LEFT);
            mKeyboardInput.bindKey(Keys.Right, InputDevice.Commands.RIGHT);
            mKeyboardInput.bindKey(Keys.Enter, InputDevice.Commands.SELECT);
            mKeyboardInput.bindKey(Keys.Escape, InputDevice.Commands.BACK);
            mKeyboardInput.bindKey(Keys.Space, InputDevice.Commands.BOOST);
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

    public void SubmitScore(int score)
    {
        mHighscores.Add(new Tuple<int, DateTime>(score, DateTime.Now));
        mHighscores.Sort((s1, s2) => s2.Item1 - s1.Item1);
        for (var i = 5; i < mHighscores.Count; i++)
        {
            mHighscores.Remove(mHighscores[i]);
        }
        StorageUtil.storeData("highscores.json", mHighscores);
    }

    public void changeState(GameStates nextState)
    {
        if (nextState == GameStates.EXIT)
        {
            Exit();
            return;
        }
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

// TODO: Add boosting for mouse and keyboard
// TODO: Add sounds
// TODO: Add better assets for snakes, food, etc
// TODO: Give the player a chance to name themselves
// TODO: Show the tutorial message/diagram "your snake will follow your mouse"
// TODO: Show the message/diagram for boosting
// TODO: Show the joining message and once joined, the player begins participating in the game
// TODO: Food refreshes periodically
// TODO: Snakes leave behind food upon dying
// TODO: Show end game panel after death that shows score, kills, and highest position achieved
// TODO: Customization of controls
// TODO: Show player name beside head of snake
// TODO: Multiple kinds and sizes of food, utilizing animated sprite
// TODO: Game status panel, showing current score and top 5 snakes in game
// TODO: Background image
// TODO: Finish server implementation
// TODO: Test multiplayer on local machine and multiple machines