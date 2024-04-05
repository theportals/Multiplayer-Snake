using System;
using System.Collections.Generic;
using System.Linq;
using Client.Components;
using Client.Entities;
using Client.Input;
using Client.Systems;
using Client.Util;
using Client.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Multiplayer_Snake.Views.Menus;
using Shared.Components;
using Shared.Entities;
using Shared.Messages;
using Shared.Util;
using Food = Shared.Entities.Food;

namespace Client;

public class GameModel
{
    private readonly int WINDOW_WIDTH;
    private readonly int WINDOW_HEIGHT;

    private List<Entity> mToRemove = new();
    private List<Entity> mToAdd = new();
    private List<Entity> mParticlesToAdd = new();

    private Dictionary<uint, Entity> mEntities;

    private Systems.Renderer mSysRenderer;
    private Systems.Movement mSysMovement;
    private Systems.Input mSysInput;
    private Systems.Lifetime mSysLifetime;
    private Systems.Lifetime mSysParticleLifetime;

    private Systems.Network mSysNetwork;
    private Systems.Interpolation mSysInterp;

    private ContentManager mContentManager;

    private Texture2D snakeSheet;
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

    private Color? lastColor = null;

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
        mPause.initialize(mGame, mGame.GraphicsDevice, Client.mGraphics, mKeyboardInput, mMouseInput);
    }

    public bool Initialize(ContentManager content, SpriteBatch spriteBatch)
    {
        mEntities = new Dictionary<uint, Entity>();
        mSpriteBatch = spriteBatch;
        mKeyboardInput.clearCommands();
        mMouseInput.clearRegions();
        mContentManager = content;
        snakeSheet = content.Load<Texture2D>("Images/Snake_Sheet");
        thrust = content.Load<SoundEffect>("Sounds/thrust");
        font = content.Load<SpriteFont>("Fonts/name");

        var bg = content.Load<Texture2D>("Images/normal_hillside");
        
        thrustInstance = thrust.CreateInstance();
        thrustInstance.IsLooped = true;

        mPause.loadContent(content);
        mPause.initializeSession();

        mSysRenderer = new Systems.Renderer(spriteBatch, font, bg, WINDOW_WIDTH, WINDOW_HEIGHT, Constants.ARENA_SIZE, null);

        mSysMovement = new Systems.Movement();
        mSysInput = new Systems.Input(mKeyboardInput, mMouseInput, mListenKeys, Constants.ARENA_SIZE, WINDOW_WIDTH, WINDOW_HEIGHT, false);
        mSysLifetime = new Systems.Lifetime(e =>
        {
            mToRemove.Add(e);
            var food = e.get<Shared.Components.Food>();
            // if (food.naturalSpawn) mToAdd.Add(createFood(true));
        });
        mSysParticleLifetime = new Systems.Lifetime(e =>
        {
            mToRemove.Add(e);
        });

        mSysNetwork = new Network();
        mSysInterp = new Interpolation();
        
        mSysNetwork.registerNewEntityHandler(handleNewEntity);
        mSysNetwork.registerRemoveEntityHandler(handleRemoveEntity);

        spawnSnake();
        mSysRenderer.zoom = 2.5f;
        mSysInput.zoom = mSysRenderer.zoom;
        mSysInput.setAbsCursor(true);
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

        return true;
    }

    public void handleNewEntity(NewEntity message)
    {
        Entity entity = createEntity(message);
        addEntity(entity);
    }

    public Entity createEntity(NewEntity message)
    {
        Entity entity = new Entity(message.id);
        // Console.WriteLine($"Adding entity with id {entity.id}");

        if (message.hasAppearance)
        {
            entity.add(new Appearance(message.texture, 
                message.displaySize,
                message.animated,
                message.frames,
                message.frameWidth,
                message.frameHeight,
                message.staticFrame));
            var texture = mContentManager.Load<Texture2D>(message.texture);
            entity.add(new Sprite(texture));
            // Console.WriteLine($"Gave entity texture");
        }

        if (message.hasPosition)
        {
            entity.add(new Position(message.segments));
        }

        if (message.hasMovement)
        {
            entity.add(new Movable(message.facing, message.moveSpeed, message.turnSpeed, message.segmentsToAdd));
        }

        if (message.hasInput)
        {
            entity.add(new Shared.Components.Input(message.inputs));
        }

        if (message.hasRotationOffset)
        {
            entity.add(new RotationOffset(message.rotationOffset, message.rotationSpeed));
        }

        if (message.hasColorOverride)
        {
            entity.add(new ColorOverride(System.Drawing.Color.FromArgb(message.cR, message.cG, message.cB)));
        }

        if (message.hasPlayername)
        {
            entity.add(new PlayerName(message.playerName));
        }

        if (message.suggestFollow)
        {
            mPlayerSnake = entity;
            mSysRenderer.follow(entity);

            // We only want to attempt to control snakes we own
            if (message.controllable)
            {
                entity.add(new Controllable());
            }
        }

        if (message.boostable)
        {
            entity.add(new Boostable(message.maxStamina, message.staminaDrain, message.speedModifier, message.regenRate,
                message.penaltySpeed, message.stamina, message.boosting));
        }

        if (message.alive)
        {
            entity.add(new Alive());
        }

        if (message.snakeitude)
        {
            entity.add(new Snakeitude());
            Console.WriteLine($"Snake has {message.segmentsToAdd} segments to add");
        }

        if (message.food)
        {
            entity.add(new Shared.Components.Food(message.naturalSpawn));
        }

        return entity;
    }

    public void handleRemoveEntity(RemoveEntity message)
    {
        removeEntity(mEntities[message.id]);
    }

    // private void playerDeath(Entity e)
    // {
    //     e.remove<Alive>();
    //     mToRemove.Add(e);
    //     if (mScore > 0) mGame.SubmitScore(mScore);
    //     var pos = e.get<Position>();
    //     for (var segment = 0; segment < pos.segments.Count; segment++)
    //     {
    //         mToAdd.Add(createFood(false, pos.segments[segment]));
    //     }
    //
    //     mLeaderboard.RemoveAll(t => t.Item1 == mGame.playerName);
    // }

    public void spawnSnake()
    {
        // Color replacement for the snake
        Color cFrom;
        if (lastColor == null) cFrom = new Color(0, 255, 0);
        else cFrom = lastColor.Value;
        var rng = new Random();
        var r = rng.Next(230);
        var g = rng.Next(230);
        var b = rng.Next(230);
        var cTo = new Color(r, g, b);
        // var cTo = new Color(255, 255, 255);
        Color[] data = new Color[snakeSheet.Width * snakeSheet.Height];
        snakeSheet.GetData(data);

        var tolerance = 15;
        for (int i = 0; i < data.Length; i++)
        {
            if (Math.Abs(data[i].R - cFrom.R) < tolerance && Math.Abs(data[i].G - cFrom.G) < tolerance && Math.Abs(data[i].B - cFrom.B) < tolerance)
            {
                data[i] = cTo;
            }
        }
        snakeSheet.SetData(data);
        lastColor = cTo;
        
        mScore = 0;
        if (mPlayerSnake != null && mPlayerSnake.contains<Alive>())
        {
            // playerDeath(mPlayerSnake);
            mPlayerSnake = null;
        }
        
        if (mListenKeys) mKeyboardInput.registerCommand(InputDevice.Commands.BOOST, 
            _ => boostOn(mPlayerSnake), 
            _ => playBoost(mPlayerSnake), 
            _ => boostOff(mPlayerSnake));
        else mMouseInput.registerMouseRegion(null, MouseInput.MouseActions.L_CLICK, 
            _ => boostOn(mPlayerSnake), 
            _ => playBoost(mPlayerSnake), 
            _ => boostOff(mPlayerSnake));

        var t = new Tuple<string, int>(Client.playerName, 0);
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
        if (mGame.IsActive && !mPause.isOpen) mSysInput.update(gameTime.ElapsedGameTime);
        else thrustInstance.Pause();
        mSysNetwork.update(gameTime.ElapsedGameTime, MessageQueueClient.instance.getMessages());
        mSysInterp.update(gameTime.ElapsedGameTime);
        mSysMovement.update(gameTime.ElapsedGameTime);
        mSysLifetime.update(gameTime.ElapsedGameTime);
        mSysParticleLifetime.update(gameTime.ElapsedGameTime);

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

    public void disconnect()
    {
        Console.WriteLine("Sending Disconnect");
        MessageQueueClient.instance.sendMessage(new Disconnect());
        MessageQueueClient.instance.submitShutdown();
        mGame.changeState(GameStates.MAIN_MENU);
    }

    public void render(GameTime gameTime)
    {
        mSysRenderer.update(gameTime.ElapsedGameTime);
        
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
        mSysMovement.add(particle);
        mSysRenderer.add(particle);
        mSysParticleLifetime.add(particle);
    }

    private void addEntity(Entity entity)
    {
        if (entity == null)
        {
            Console.WriteLine("Attempted to add a null entity...");
            return;
        }

        mEntities[entity.id] = entity;
        mSysMovement.add(entity);
        mSysRenderer.add(entity);
        mSysInput.add(entity);
        mSysLifetime.add(entity);
        mSysNetwork.add(entity);
        mSysInterp.add(entity);
    }

    private void removeEntity(Entity entity)
    {
        mEntities.Remove(entity.id);
        mSysMovement.remove(entity.id);
        mSysRenderer.remove(entity.id);
        mSysInput.remove(entity.id);
        mSysParticleLifetime.remove(entity.id);
        mSysNetwork.remove(entity.id);
        mSysInterp.remove(entity.id);
    }
}