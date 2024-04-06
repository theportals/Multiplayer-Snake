using System.Numerics;
using System.Text;
using Shared.Components;
using Shared.Entities;
using Shared.Messages;
using Shared.Systems;
using Shared.Util;
using Collision = Server.Systems.Collision;
using Food = Shared.Entities.Food;
using Lifetime = Shared.Systems.Lifetime;

namespace Server;

public class GameModel
{
    
    private HashSet<int> mClients = new HashSet<int>();
    private Dictionary<uint, Entity> mEntities = new Dictionary<uint, Entity>();
    private Dictionary<int, uint> mClientToEntityId = new Dictionary<int, uint>();

    private List<Entity> mToAdd = new();
    private List<Entity> mToRemove = new();

    Systems.Network mSystemNetwork = new Server.Systems.Network();
    private Collision mSysCollision;
    private Movement mSysMovement;
    private Shared.Systems.Lifetime mSysLifetime;

    /// <summary>
    /// This is where the server-side simulation takes place.  Messages
    /// from the network are processed and then any necessary client
    /// updates are sent out.
    /// </summary>
    public void update(TimeSpan elapsedTime)
    {
        mSystemNetwork.update(elapsedTime, MessageQueueServer.instance.getMessages());
        
        mSysCollision.update(elapsedTime);
        mSysMovement.update(elapsedTime);
        mSysLifetime.update(elapsedTime);

        foreach (var entity in mToRemove)
        {
            removeEntity(entity.id);
        }
        mToRemove.Clear();

        foreach (var entity in mToAdd)
        {
            addEntity(entity);
        }
        mToAdd.Clear();
    }

    public bool initialize()
    {
        mSystemNetwork.registerJoinHandler(handleJoin);
        mSystemNetwork.registerDisconnectHandler(handleDisconnect);
        
        MessageQueueServer.instance.registerConnectHandler(handleConnect);

        mSysCollision = new Collision(food =>
        {
            // Called when player collides with food
            mToRemove.Add(food);
            // TODO: Better food consumption
            MessageQueueServer.instance.broadcastMessage(new RemoveEntity(food.id));
            // score.Play();
            // mToRemove.Add(e);
            // mScore += 1;
            // mLeaderboard.RemoveAll(t => t.Item1 == mGame.playerName);
            // var t = new Tuple<string, int>(mGame.playerName, mScore);
            // mLeaderboard.Add(t);
            // mLeaderboard.Sort((t1, t2) => t2.Item2 - t1.Item2);
            // var rank = mLeaderboard.IndexOf(t) + 1;
            // if (mBestRank > rank) mBestRank = rank;
            // addParticlesLater(ParticleUtil.eatFood(foodSheet, e));
        }, e =>
        {
            // Called when player collides with non-food
            // TODO: Player collision
            // mPlayerSnake = null;
            // mPause.gameOver = true;
            // mPause.open();
            // playerDeath(e);
            // addParticlesLater(ParticleUtil.playerDeath(fire, smoke, e));
            // thrustInstance.Pause();
            // explode.Play();
        });

        mSysMovement = new Movement();

        mSysLifetime = new Shared.Systems.Lifetime(food =>
        {
            mToRemove.Add(food);
            if (food.get<Shared.Components.Food>().naturalSpawn) mToAdd.Add(createFood(true));
            MessageQueueServer.instance.broadcastMessage(new RemoveEntity(food.id));
        });
        
        // Initialize arena
        initializeBorder();
        initializeObstacles();
        for (var i = 0; i < Constants.FOOD_COUNT; i++)
        {
            addEntity(createFood());
        }
        
        Console.WriteLine();
        return true;
    }

    public void shutdown()
    {
        
    }

    private void handleConnect(int clientId)
    {
        mClients.Add(clientId);
        
        MessageQueueServer.instance.sendMessage(clientId, new Shared.Messages.ConnectAck());
    }

    private void handleDisconnect(int clientId)
    {
        mClients.Remove(clientId);
        if (mClientToEntityId.ContainsKey(clientId))
        {
            var message = new Shared.Messages.RemoveEntity(mClientToEntityId[clientId]);
            MessageQueueServer.instance.broadcastMessage(message);
            removeEntity(mClientToEntityId[clientId]);

            mClientToEntityId.Remove(clientId);
        }
    }

    private void addEntity(Entity entity)
    {
        if (entity == null)
        {
            return;
        }

        mEntities[entity.id] = entity;
        mSystemNetwork.add(entity);
        mSysCollision.add(entity);
        mSysMovement.add(entity);
        mSysLifetime.add(entity);
    }

    private void removeEntity(uint id)
    {
        mEntities.Remove(id);
        mSystemNetwork.remove(id);
        mSysCollision.remove(id);
        mSysMovement.remove(id);
        mSysLifetime.remove(id);
    }

    private void reportAllEntities(int clientId)
    {
        foreach (var item in mEntities)
        {
            MessageQueueServer.instance.sendMessage(clientId, new Shared.Messages.NewEntity(item.Value));
        }
    }

    private void handleJoin(int clientId, string playerName)
    {
        Console.WriteLine($"{playerName} joined");
        // Step 1: Tell the newly connected player about all other entities
        reportAllEntities(clientId);
        spawnSnake(clientId, playerName);
    }

    private void spawnSnake(int clientId, string playerName)
    {
        // Step 2: Create an entity for the newly joined player and send it to the newly joined client
        // TODO: Player spawns in the least populated area
        var snake = SnakeSegment.create("Images/Snake_Sheet", 500, 500, 3, playerName);
        addEntity(snake);
        mClientToEntityId[clientId] = snake.id;
        
        // Step 3: Send the new snake to the newly joined client
        MessageQueueServer.instance.sendMessage(clientId, new NewEntity(snake, true));
        
        // Step 4: Let all other clients know about the new entity
        // Remove components not needed for other players
        snake.remove<Shared.Components.Controllable>();
        
        var message = new NewEntity(snake);
        foreach (int otherId in mClients)
        {
            if (otherId != clientId)
            {
                MessageQueueServer.instance.sendMessage(otherId, message);
            }
        }
    }

    private void initializeBorder()
    {
        for (int position = 0; position < Constants.ARENA_SIZE; position += 10)
        {
            var left = BorderBlock.create("Images/square", 0, position);
            addEntity(left);
            
            var right = BorderBlock.create("Images/square", Constants.ARENA_SIZE, position);
            addEntity(right);
            
            var top = BorderBlock.create("Images/square", position, 0);
            addEntity(top);
            
            var bottom = BorderBlock.create("Images/square", position, Constants.ARENA_SIZE);
            addEntity(bottom);
        
            var bottomRight = BorderBlock.create("Images/square", Constants.ARENA_SIZE, Constants.ARENA_SIZE);
            addEntity(bottomRight);
        }
    }

    private void initializeObstacles()
    {
        var rng = new ExtendedRandom();
        var remaining = Constants.OBSTACLE_COUNT;

        while (remaining > 0)
        {
            int x = (int)rng.nextRange(1, Constants.ARENA_SIZE - 1);
            int y = (int)rng.nextRange(1, Constants.ARENA_SIZE - 1);
            var proposed = Obstacle.create("Images/bomb", x, y);
            if (!mSysCollision.anyCollision(proposed))
            {
                addEntity(proposed);
                remaining -= 1;
            }
        }
    }

    private Entity createFood(bool naturalSpawn=true, Vector2? pos=null)
    {
        var rng = new ExtendedRandom();
        var done = false;

        while (!done)
        {
            if (pos != null)
            {
                var food = Food.create("Images/Food_Sheet", (int)pos.Value.X, (int)pos.Value.Y, naturalSpawn);
                return food;
            }
            int x = (int)rng.nextRange(1, Constants.ARENA_SIZE - 1);
            int y = (int)rng.nextRange(1, Constants.ARENA_SIZE - 1);
            var proposed = Food.create("Images/Food_Sheet", x, y, naturalSpawn);
            if (!mSysCollision.anyCollision(proposed))
            {
                MessageQueueServer.instance.broadcastMessage(new NewEntity(proposed));
                return proposed;
            }
        }

        return null;
    }

    private Entity createSnake()
    {
        var rng = new ExtendedRandom();
        bool done = false;

        while (!done)
        {
            int x = (int)rng.nextRange(1, Constants.ARENA_SIZE - 1);
            int y = (int)rng.nextRange(1, Constants.ARENA_SIZE - 1);
            var proposed = SnakeSegment.create("Images/Snake_Sheet", x, y);
            if (!mSysCollision.anyCollision(proposed))
            {
                addEntity(proposed);
                proposed.get<Shared.Components.Movable>().segmentsToAdd = 3;
                return proposed;
            }
        }

        return null;
    }
}