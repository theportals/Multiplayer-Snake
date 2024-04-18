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
using Shared;
using Shared.Components;
using Shared.Entities;
using Shared.Messages;
using Shared.Systems;
using Shared.Util;
using Food = Shared.Entities.Food;
using Lifetime = Shared.Systems.Lifetime;

namespace Client;

public class GameModel
{
    private readonly int WINDOW_WIDTH;
    private readonly int WINDOW_HEIGHT;

    private List<Entity> mToRemove = new();
    private List<Entity> mToAdd = new();
    private List<Entity> mParticlesToAdd = new();

    private Dictionary<uint, Entity> mClientIdToEntity = new();
    private Dictionary<uint, Entity> mServerIdToEntity = new();

    private Systems.Renderer mSysRenderer;
    private Movement mSysMovement;
    private Systems.Input mSysInput;
    private Lifetime mSysLifetime;

    private Lifetime mSysParticleLifetime;

    private Systems.Network mSysNetwork;
    private Systems.Interpolation mSysInterp;

    private ContentManager mContentManager;

    private Texture2D fire;
    private Texture2D smoke;
    private Texture2D snakeSheet;
    private Texture2D foodSheet;
    private SoundEffect onScore;
    private SoundEffect explode;
    private SoundEffect thrust;
    private SoundEffectInstance thrustInstance;
    private SpriteFont font;

    private SpriteBatch mSpriteBatch;

    private KeyboardInput mKeyboardInput;
    private MouseInput mMouseInput;
    private bool mListenKeys;

    public int mScore;
    public int mKills = 0;
    public int mBestRank = 0;

    private Color? lastColor = null;

    public List<Tuple<uint, int>> mLeaderboard = new();

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
        mClientIdToEntity = new Dictionary<uint, Entity>();
        mServerIdToEntity = new Dictionary<uint, Entity>();
        mSpriteBatch = spriteBatch;
        mKeyboardInput.clearCommands();
        mMouseInput.clearRegions();
        mContentManager = content;
        fire = content.Load<Texture2D>("Particles/fire");
        smoke = content.Load<Texture2D>("Particles/smoke-2");
        snakeSheet = content.Load<Texture2D>("Images/Snake_Sheet");
        foodSheet = content.Load<Texture2D>("Images/Food_Sheet");
        onScore = content.Load<SoundEffect>("Sounds/score");
        thrust = content.Load<SoundEffect>("Sounds/thrust");
        explode = content.Load<SoundEffect>("Sounds/explosion");
        font = content.Load<SpriteFont>("Fonts/name");

        var bg = content.Load<Texture2D>("Images/normal_hillside");
        
        thrustInstance = thrust.CreateInstance();
        thrustInstance.IsLooped = true;

        mPause.loadContent(content);
        mPause.initializeSession();

        mSysRenderer = new Systems.Renderer(spriteBatch, font, bg, WINDOW_WIDTH, WINDOW_HEIGHT, Constants.ARENA_SIZE, null);

        mSysMovement = new Movement();
        mSysInput = new Systems.Input(mKeyboardInput, mMouseInput, mListenKeys, Constants.ARENA_SIZE, WINDOW_WIDTH, WINDOW_HEIGHT, false);
        mSysLifetime = new Lifetime(e =>
        {
            mToRemove.Add(e);
        });
        mSysParticleLifetime = new Lifetime(e =>
        {
            mToRemove.Add(e);
        });

        mSysNetwork = new Network();
        mSysInterp = new Interpolation();
        
        mSysNetwork.registerNewEntityHandler(handleNewEntity);
        mSysNetwork.registerRemoveEntityHandler(handleRemoveEntity);

        bindBoost();
        mSysRenderer.zoom = 2.5f;
        mSysInput.zoom = mSysRenderer.zoom;
        mSysInput.setAbsCursor(true);
        mKeyboardInput.registerCommand(InputDevice.Commands.BACK, _ =>
        {
            mPause.toggle();
            if (mPlayerSnake != null) boostOff();
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

    public void bindBoost()
    {
        if (mListenKeys) mKeyboardInput.registerCommand(InputDevice.Commands.BOOST, 
            _ => boostOn(), 
            _ => playBoost(), 
            _ => boostOff());
        else mMouseInput.registerMouseRegion(null, MouseInput.MouseActions.L_CLICK, 
            _ => boostOn(), 
            _ => playBoost(), 
            _ => boostOff());
    }

    public void handleNewEntity(NewEntity message)
    {
        var entity = createEntity(message);
        mServerIdToEntity[message.id] = entity;
        mClientIdToEntity[entity.id] = entity;
        mSysNetwork.mapServerToClientId(message.id, entity.id);
        mSysInput.mapClientToServerId(entity.id, message.id);
        addEntity(entity);
    }

    public Entity createEntity(NewEntity message)
    {
        var entity = new Entity();

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

            if (message.snakeitude)
            {
                var cFrom = new Color(0, 255, 0);
                var rng = new Random();
                var r = rng.Next(230);
                var g = rng.Next(230);
                var b = rng.Next(230);
                var cTo = new Color(r, g, b);
                var data = new Color[snakeSheet.Width * snakeSheet.Height];
                texture.GetData(data);

                var recolor = new Texture2D(mSpriteBatch.GraphicsDevice, snakeSheet.Width, snakeSheet.Height);

                var tolerance = 15;
                var count = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    if (Math.Abs(data[i].R - cFrom.R) < tolerance && Math.Abs(data[i].G - cFrom.G) < tolerance && Math.Abs(data[i].B - cFrom.B) < tolerance)
                    {
                        data[i] = cTo;
                        count += 1;
                    }
                }
                recolor.SetData(data);
                entity.add(new Sprite(recolor));
            }
            
            else entity.add(new Sprite(texture));
        }

        if (message.hasPosition)
        {
            entity.add(new Position(message.segments));
        }

        if (message.hasMovement)
        {
            entity.add(new Movable(message.facing, message.moveSpeed, message.turnSpeed, message.segmentsToAdd));
        }

        if (message.hasRotationOffset)
        {
            entity.add(new RotationOffset(message.rotationOffset, message.rotationSpeed));
        }

        if (message.hasColorOverride)
        {
            entity.add(new ColorOverride(System.Drawing.Color.FromArgb(message.cR, message.cG, message.cB)));
        }

        if (message.hasPlayerInfo)
        {
            entity.add(new PlayerInfo(message.playerName, message.score, message.kills));
            mLeaderboard.Add(new Tuple<uint, int>(entity.id, message.score));
            mLeaderboard.Sort((t1, t2) => t2.Item2 - t1.Item2);
        }

        if (message.suggestFollow)
        {
            mPlayerSnake = entity;
            mSysRenderer.follow(entity);
            // We've been given a new snake, so reset stats
            mKills = 0;
            mScore = 0;
            mBestRank = mLeaderboard.Count;
        }

        if (message.controllable)
        {
            entity.add(new Controllable());
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
        }

        if (message.food)
        {
            entity.add(new Shared.Components.Food(message.naturalSpawn));
        }

        if (message.collision)
        {
            entity.add(new Collision(message.collisionSize, message.intangibility));
        }

        return entity;
    }

    public void handleRemoveEntity(RemoveEntity message)
    {
        if (!mServerIdToEntity.ContainsKey(message.removeId))
        {
            Console.WriteLine("[WARN]: Received a remove message for an entity that did not receive a create message");
            return;
        }

        var entity = mServerIdToEntity[message.removeId];
        removeEntity(entity);
        switch (message.reason)
        {
            case RemoveEntity.Reasons.PLAYER_DIED:
                if (entity.Equals(mPlayerSnake))
                {
                    playerDeath(entity);
                }
                else
                {
                    addParticlesLater(ParticleUtil.enemyDeath(fire, smoke, entity));
                    mLeaderboard.RemoveAll(t => t.Item1 == entity.id);

                    if (message.reasonId.HasValue
                        && mServerIdToEntity.ContainsKey(message.reasonId.Value)
                        && mServerIdToEntity[message.reasonId.Value].Equals(mPlayerSnake))
                    {
                        mKills += 1;
                    }
                }
                break;
            case RemoveEntity.Reasons.PLAYER_RESPAWNED:
                mLeaderboard.RemoveAll(t => t.Item1 == entity.id);
                break;
            case RemoveEntity.Reasons.PLAYER_DISCONNECT:
                mLeaderboard.RemoveAll(t => t.Item1 == entity.id);
                break;
            case RemoveEntity.Reasons.FOOD_CONSUMED:
                if (message.reasonId.HasValue && mServerIdToEntity.ContainsKey(message.reasonId.Value))
                {
                    var cause = mServerIdToEntity[message.reasonId.Value];
                    mLeaderboard.RemoveAll(t => t.Item1 == cause.id);
                    var info = cause.get<PlayerInfo>();
                    var t = new Tuple<uint, int>(cause.id, info.score);
                    mLeaderboard.Add(t);
                    mLeaderboard.Sort((t1, t2) => t2.Item2 - t1.Item2);
                    if (cause.Equals(mPlayerSnake))
                    {
                        onScore.Play();
                            
                        addParticlesLater(ParticleUtil.eatFood(foodSheet, entity));
                        mScore = info.score;
                        var rank = mLeaderboard.IndexOf(t) + 1;
                        if (mBestRank > rank) mBestRank = rank;
                    }
                    
                }
                else
                {
                    Console.WriteLine("[WARN]: Removal cause entity did not receive a create message");
                }
                break;
            case RemoveEntity.Reasons.FOOD_EXPIRED:
                // Nothing special needs to be done on our end
                break;
            default:
                Console.WriteLine($"[WARN]: GameModel has no response for RemoveEntity Reason: {message.reason.ToString()}");
                break;
        }
    }

    private void playerDeath(Entity e)
    {
        e.remove<Alive>();
        addParticlesLater(ParticleUtil.playerDeath(fire, smoke, e));
        explode.Play();
        mToRemove.Add(e);
        if (mScore > 0) mGame.SubmitScore(mScore);

        mPause.gameOver = true;
        mPause.open();
    
        mLeaderboard.RemoveAll(t => t.Item1 == e.id);
    }

    private void boostOn()
    {
        if (mPlayerSnake != null && mPlayerSnake.contains<Alive>())
        {
            mPlayerSnake.get<Boostable>().boosting = true;
        }
    }

    private void playBoost()
    {
        if (mPlayerSnake != null && mPlayerSnake.contains<Alive>() && mPlayerSnake.get<Boostable>().stamina > 0) thrustInstance.Play();
        else thrustInstance.Pause();
    }
    
    private void boostOff()
    {
        thrustInstance.Pause();
        if (mPlayerSnake != null && mPlayerSnake.contains<Alive>())
        {
            mPlayerSnake.get<Boostable>().boosting = false;
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
            var (id, score) = mLeaderboard[i];
            size = font.MeasureString(score.ToString());

            var info = mClientIdToEntity[id].get<PlayerInfo>();
            
            mSpriteBatch.DrawString(font, info.playerName, new Vector2(x + padding, y + padding + i * font.LineSpacing), Color.White);
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
        mClientIdToEntity[particle.id] = particle;
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

        mClientIdToEntity[entity.id] = entity;
        mSysMovement.add(entity);
        mSysRenderer.add(entity);
        mSysInput.add(entity);
        mSysLifetime.add(entity);
        mSysNetwork.add(entity);
        mSysInterp.add(entity);
    }

    private void removeEntity(Entity entity)
    {
        mClientIdToEntity.Remove(entity.id);
        mSysMovement.remove(entity.id);
        mSysRenderer.remove(entity.id);
        mSysInput.remove(entity.id);
        mSysParticleLifetime.remove(entity.id);
        mSysNetwork.remove(entity.id);
        mSysInterp.remove(entity.id);
    }
}