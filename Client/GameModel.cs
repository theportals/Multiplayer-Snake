using System.Collections.Generic;
using Client.Components;
using Client.Entities;
using Client.Input;
using Client.Util;
using Client.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;
using Shared.Entities;
using Food = Client.Entities.Food;

namespace Client;

public class GameModel
{
    private const int ARENA_SIZE = 1000;
    private const int OBSTACLE_COUNT = 5;
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

    private KeyboardInput mKeyboardInput;
    private MouseInput mMouseInput;
    private bool mListenKeys;

    private int mScore;

    private Client mGame;

    public GameModel(Client game, int width, int height, KeyboardInput keyboardInput, MouseInput mouseInput, bool listenKeys)
    {
        mGame = game;
        WINDOW_WIDTH = width;
        WINDOW_HEIGHT = height;
        mKeyboardInput = keyboardInput;
        mMouseInput = mouseInput;
        mListenKeys = listenKeys;
    }

    public void Initialize(ContentManager content, SpriteBatch spriteBatch)
    {
        mKeyboardInput.clearCommands();
        mMouseInput.clearRegions();
        square = content.Load<Texture2D>("Images/square");
        peepo = content.Load<Texture2D>("Images/Peepo");
        fire = content.Load<Texture2D>("Particles/fire");
        smoke = content.Load<Texture2D>("Particles/smoke-2");
        explode = content.Load<SoundEffect>("Sounds/explosion");
        score = content.Load<SoundEffect>("Sounds/score");
        thrust = content.Load<SoundEffect>("Sounds/thrust");
        thrustInstance = thrust.CreateInstance();
        thrustInstance.IsLooped = true;

        mSysRenderer = new Systems.Renderer(spriteBatch, square, WINDOW_WIDTH, WINDOW_HEIGHT, ARENA_SIZE, null);
        mSysCollision = new Systems.Collision(e =>
        {
            score.Play();
            mToRemove.Add(e);
            mScore += 1;
        },
        e =>
        {
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
        addEntity(createFood(square));
        addEntity(createFood(square));
        addEntity(createFood(square));
        addEntity(createFood(square));
        addEntity(createFood(square));
        mKeyboardInput.registerCommand(InputDevice.Commands.BACK, _ => mGame.changeState(GameStates.MAIN_MENU));
        
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
        mScore = 0;
        var pos = e.get<Position>();
        foreach (var segment in pos.segments)
        {
            mToAdd.Add(createFood(square, false, segment));
        }
        
        if (mListenKeys) mKeyboardInput.registerCommand(InputDevice.Commands.SELECT, _ => spawnSnake());
        mMouseInput.registerMouseRegion(null, MouseInput.MouseActions.L_CLICK, _ => spawnSnake());
    }

    private void spawnSnake()
    {
        var snake = initializeSnake(square);
        mSysRenderer.follow(snake);
        
        
        if (mListenKeys) mKeyboardInput.registerCommand(InputDevice.Commands.BOOST, 
            _ => boostOn(snake), 
            _ => playBoost(snake), 
            _ => boostOff(snake));
        else mMouseInput.registerMouseRegion(null, MouseInput.MouseActions.L_CLICK, 
            _ => boostOn(snake), 
            _ => playBoost(snake), 
            _ => boostOff(snake));
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
        if (snake.contains<Alive>())
        {
            thrustInstance.Pause();
            snake.get<Boostable>().boosting = false;
        }
    }

    public void update(GameTime gameTime)
    {
        if (mGame.IsActive) mSysInput.Update(gameTime.ElapsedGameTime);
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
    }

    public void Draw(GameTime gameTime)
    {
        mSysRenderer.Update(gameTime.ElapsedGameTime);
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