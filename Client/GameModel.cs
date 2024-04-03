using System;
using System.Collections.Generic;
using System.Linq;
using Client.Components;
using Client.Entities;
using Client.Input;
using Client.Util;
using Client.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Multiplayer_Snake.Views.Menus;
using Shared.Components;
using Shared.Entities;
using Food = Client.Entities.Food;

namespace Client;

public class GameModel
{
    private const int ARENA_SIZE = 1000;
    private const int OBSTACLE_COUNT = 200;
    private const int FOOD_COUNT = 25;
    private readonly int WINDOW_WIDTH;
    private readonly int WINDOW_HEIGHT;

    private List<Entity> mToRemove = new();
    private List<Entity> mToAdd = new();
    private List<Entity> mParticlesToAdd = new();

    private Systems.Renderer mSysRenderer;
    private Systems.Collision mSysCollision;
    private Systems.Movement mSysMovement;
    private Systems.Input mSysInput;
    private Systems.Lifetime mSysLifetime;
    private Systems.Lifetime mSysParticleLifetime;

    private Texture2D square;
    private Texture2D fire;
    private Texture2D smoke;
    private Texture2D peepo;
    private SoundEffect explode;
    private SoundEffect score;
    private SoundEffect thrust;
    private SoundEffectInstance thrustInstance;
    private SpriteFont font;

    private SpriteBatch mSpriteBatch;

    private KeyboardInput mKeyboardInput;
    private MouseInput mMouseInput;
    private bool mListenKeys;

    public int mScore;
    public int mKills = 0; // TODO: Track kills
    public int mBestRank = 0;

    public List<Tuple<string, int>> mLeaderboard = new()
    {
        new Tuple<string, int>("Test1", 0),
        new Tuple<string, int>("Test2", 5),
        new Tuple<string, int>("Test3", 30),
        new Tuple<string, int>("Test4", 100),
        new Tuple<string, int>("Test5", 50)
    };

    private Client mGame;

    private PauseMenu mPause;

    private Entity? mPlayerSnake;

    public GameModel(Client game, int width, int height, KeyboardInput keyboardInput, MouseInput mouseInput, bool listenKeys)
    {
        mGame = game;
        WINDOW_WIDTH = width;
        WINDOW_HEIGHT = height;
        mKeyboardInput = keyboardInput;
        mMouseInput = mouseInput;
        mListenKeys = listenKeys;
        mPause = new PauseMenu(this);
        mPause.initialize(mGame, mGame.GraphicsDevice, mGame.mGraphics, mKeyboardInput, mMouseInput);
    }

    public void Initialize(ContentManager content, SpriteBatch spriteBatch)
    {
        mSpriteBatch = spriteBatch;
        mKeyboardInput.clearCommands();
        mMouseInput.clearRegions();
        square = content.Load<Texture2D>("Images/square");
        peepo = content.Load<Texture2D>("Images/Peepo");
        fire = content.Load<Texture2D>("Particles/fire");
        smoke = content.Load<Texture2D>("Particles/smoke-2");
        explode = content.Load<SoundEffect>("Sounds/explosion");
        score = content.Load<SoundEffect>("Sounds/score");
        thrust = content.Load<SoundEffect>("Sounds/thrust");
        font = content.Load<SpriteFont>("Fonts/name");
        thrustInstance = thrust.CreateInstance();
        thrustInstance.IsLooped = true;

        mPause.loadContent(content);
        mPause.initializeSession();

        mSysRenderer = new Systems.Renderer(spriteBatch, font, square, WINDOW_WIDTH, WINDOW_HEIGHT, ARENA_SIZE, null);
        mSysCollision = new Systems.Collision(e =>
        {
            score.Play();
            mToRemove.Add(e);
            mScore += 1;
            mLeaderboard.RemoveAll(t => t.Item1 == mGame.playerName);
            var t = new Tuple<string, int>(mGame.playerName, mScore);
            mLeaderboard.Add(t);
            mLeaderboard.Sort((t1, t2) => t2.Item2 - t1.Item2);
            var rank = mLeaderboard.IndexOf(t) + 1;
            if (mBestRank > rank) mBestRank = rank;
        },
        e =>
        {
            mPlayerSnake = null;
            mPause.gameOver = true;
            mPause.open();
            playerDeath(e);
            addParticlesLater(ParticleUtil.playerDeath(fire, smoke, e));
            thrustInstance.Pause();
            explode.Play();
        });

        mSysMovement = new Systems.Movement();
        mSysInput = new Systems.Input(mKeyboardInput, mMouseInput, mListenKeys, ARENA_SIZE, WINDOW_WIDTH, WINDOW_HEIGHT, false);
        mSysLifetime = new Systems.Lifetime(e =>
        {
            mToRemove.Add(e);
            var food = e.get<Components.Food>();
            if (food.naturalSpawn) mToAdd.Add(createFood(square, true));
        });
        mSysParticleLifetime = new Systems.Lifetime(e =>
        {
            mToRemove.Add(e);
        });

        initializeBorder(square);
        initializeObstacles(square);
        spawnSnake();
        mSysRenderer.zoom = 2.5f;
        mSysInput.zoom = mSysRenderer.zoom;
        mSysInput.setAbsCursor(true);
        for (var i = 0; i < FOOD_COUNT; i++)
        {
            addEntity(createFood(square));
        }
        mKeyboardInput.registerCommand(InputDevice.Commands.BACK, _ =>
        {
            mPause.toggle();
            if (mPlayerSnake != null) boostOff(mPlayerSnake);
        });
        
        mMouseInput.registerMouseRegion(null, MouseInput.MouseActions.SCROLL_UP, _ =>
        {
            mSysRenderer.zoom *= 1.1f;
            mSysInput.zoom = mSysRenderer.zoom;
        });
        mMouseInput.registerMouseRegion(null, MouseInput.MouseActions.SCROLL_DOWN, _ =>
        {
            mSysRenderer.zoom /= 1.1f;
            mSysInput.zoom = mSysRenderer.zoom;
        });
    }

    private void playerDeath(Entity e)
    {
        e.remove<Alive>();
        mToRemove.Add(e);
        if (mScore > 0) mGame.SubmitScore(mScore);
        var pos = e.get<Position>();
        for (var segment = 0; segment < pos.segments.Count; segment++)
        {
            mToAdd.Add(createFood(square, false, pos.segments[segment]));
        }

        mLeaderboard.RemoveAll(t => t.Item1 == mGame.playerName);
    }

    public void spawnSnake()
    {
        mScore = 0;
        if (mPlayerSnake != null && mPlayerSnake.contains<Alive>())
        {
            playerDeath(mPlayerSnake);
            mPlayerSnake = null;
        }
        var snake = initializeSnake(square);
        snake.get<PlayerName>().playerName = mGame.playerName;
        mSysRenderer.follow(snake);

        mPlayerSnake = snake;
        
        if (mListenKeys) mKeyboardInput.registerCommand(InputDevice.Commands.BOOST, 
            _ => boostOn(snake), 
            _ => playBoost(snake), 
            _ => boostOff(snake));
        else mMouseInput.registerMouseRegion(null, MouseInput.MouseActions.L_CLICK, 
            _ => boostOn(snake), 
            _ => playBoost(snake), 
            _ => boostOff(snake));

        var t = new Tuple<string, int>(mGame.playerName, 0);
        mLeaderboard.Add(t);
        mLeaderboard.Sort((t1, t2) => t2.Item2 - t1.Item2);
        mBestRank = mLeaderboard.IndexOf(t);
    }

    private void boostOn(Entity snake)
    {
        if (snake.contains<Alive>())
        {
            snake.get<Boostable>().boosting = true;
        }
    }

    private void playBoost(Entity snake)
    {
        if (snake.contains<Alive>() && snake.get<Boostable>().stamina > 0) thrustInstance.Play();
        else thrustInstance.Pause();
    }
    
    private void boostOff(Entity snake)
    {
        thrustInstance.Pause();
        if (snake.contains<Alive>())
        {
            snake.get<Boostable>().boosting = false;
        }
    }

    public void update(GameTime gameTime)
    {
        if (mGame.IsActive && !mPause.isOpen) mSysInput.Update(gameTime.ElapsedGameTime);
        else thrustInstance.Pause();
        mSysMovement.Update(gameTime.ElapsedGameTime);
        mSysCollision.Update(gameTime.ElapsedGameTime);
        mSysLifetime.Update(gameTime.ElapsedGameTime);
        mSysParticleLifetime.Update(gameTime.ElapsedGameTime);

        foreach (var entity in mToRemove)
        {
            removeEntity(entity);
        }
        mToRemove.Clear();

        foreach (var entity in mToAdd)
        {
            addEntity(entity);
        }
        mToAdd.Clear();

        foreach (var particle in mParticlesToAdd)
        {
            addParticle(particle);
        }
        mParticlesToAdd.Clear();
        
        mPause.update(gameTime);
    }

    public void render(GameTime gameTime)
    {
        mSysRenderer.Update(gameTime.ElapsedGameTime);
        
        // Draw leaderboard
        mSpriteBatch.Begin();
        var x = 7 * WINDOW_WIDTH / 8 - 25;
        var y = 25;
        var w = WINDOW_WIDTH / 8;
        var h = WINDOW_WIDTH / 4;
        var padding = 25;
        mSpriteBatch.Draw(Client.pixel, new Rectangle(x, y, w, h), new Color(Color.Black, 0.5f));
        mSpriteBatch.DrawString(font, "You", new Vector2(x + padding, y + h - padding - font.LineSpacing), Color.Red);
        var size = font.MeasureString(mScore.ToString());
        mSpriteBatch.DrawString(font, mScore.ToString(), new Vector2(x + w - padding - size.X, y + h - padding - font.LineSpacing), Color.Red);
        for (int i = 0; i < Math.Min(10, mLeaderboard.Count); i++)
        {
            var (name, score) = mLeaderboard[i];
            size = font.MeasureString(score.ToString());
            
            mSpriteBatch.DrawString(font, name, new Vector2(x + padding, y + padding + i * font.LineSpacing), Color.White);
            mSpriteBatch.DrawString(font, score.ToString(), new Vector2(x + w - size.X - padding, y + padding + i * font.LineSpacing), Color.White);
        }
        mSpriteBatch.End();
        
        mPause.render(gameTime);
    }

    private void addParticlesLater(List<Entity> entities)
    {
        foreach (var entity in entities)
        {
            mParticlesToAdd.Add(entity);
        }
    }

    private void addParticle(Entity particle)
    {
        mSysMovement.Add(particle);
        mSysRenderer.Add(particle);
        mSysParticleLifetime.Add(particle);
    }

    private void addEntity(Entity entity)
    {
        mSysMovement.Add(entity);
        mSysCollision.Add(entity);
        mSysRenderer.Add(entity);
        mSysInput.Add(entity);
        mSysLifetime.Add(entity);
    }

    private void removeEntity(Entity entity)
    {
        mSysMovement.Remove(entity.Id);
        mSysCollision.Remove(entity.Id);
        mSysRenderer.Remove(entity.Id);
        mSysInput.Remove(entity.Id);
        mSysParticleLifetime.Remove(entity.Id);
    }

    private void initializeBorder(Texture2D square)
    {
        for (int position = 0; position < ARENA_SIZE; position += 10)
        {
            var left = BorderBlock.create(square, 0, position);
            addEntity(left);
            
            var right = BorderBlock.create(square, ARENA_SIZE, position);
            addEntity(right);
            
            var top = BorderBlock.create(square, position, 0);
            addEntity(top);
            
            var bottom = BorderBlock.create(square, position, ARENA_SIZE);
            addEntity(bottom);

            var bottomRight = BorderBlock.create(square, ARENA_SIZE, ARENA_SIZE);
            addEntity(bottomRight);
        }
    }

    private void initializeObstacles(Texture2D square)
    {
        var rng = new ExtendedRandom();
        var remaining = OBSTACLE_COUNT;

        while (remaining > 0)
        {
            int x = (int)rng.nextRange(1, ARENA_SIZE - 1);
            int y = (int)rng.nextRange(1, ARENA_SIZE - 1);
            var proposed = Obstacle.create(square, x, y);
            if (!mSysCollision.anyCollision(proposed))
            {
                addEntity(proposed);
                remaining -= 1;
            }
        }
    }

    private Entity initializeSnake(Texture2D square)
    {
        var rng = new ExtendedRandom();
        bool done = false;

        while (!done)
        {
            int x = (int)rng.nextRange(1, ARENA_SIZE - 1);
            int y = (int)rng.nextRange(1, ARENA_SIZE - 1);
            var proposed = SnakeSegment.create(square, x, y);
            if (!mSysCollision.anyCollision(proposed))
            {
                addEntity(proposed);
                proposed.get<Shared.Components.Movable>().segmentsToAdd = 3;
                return proposed;
            }
        }

        return null;
    }

    private Entity createFood(Texture2D square, bool naturalSpawn=true, Vector2? pos=null)
    {
        var rng = new ExtendedRandom();
        var done = false;

        while (!done)
        {
            if (pos != null)
            {
                return Food.create(square, (int)pos.Value.X, (int)pos.Value.Y, naturalSpawn);
            }
            int x = (int)rng.nextRange(1, ARENA_SIZE - 1);
            int y = (int)rng.nextRange(1, ARENA_SIZE - 1);
            var proposed = Food.create(square, x, y, naturalSpawn);
            if (!mSysCollision.anyCollision(proposed))
            {
                return proposed;
            }
        }

        return null;
    }
}