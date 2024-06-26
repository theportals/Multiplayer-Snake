using System.Numerics;
using System.Text;
using Shared;
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
        mSystemNetwork.registerRespawnHandler(handleRespawn);
        
        MessageQueueServer.instance.registerConnectHandler(handleConnect);

        mSysCollision = new Collision((food, consumer) =>
        {
            // Called when player collides with food
            mToRemove.Add(food);
            MessageQueueServer.instance.broadcastMessage(new RemoveEntity(food.id, RemoveEntity.Reasons.FOOD_CONSUMED, consumer.id));
            consumer.get<PlayerInfo>().score += 1;
            if (food.get<Shared.Components.Food>().naturalSpawn) mToAdd.Add(createFood());
        }, (collider, collidee) =>
        {
            // Called when player collides with non-food
            mToRemove.Add(collider);
            MessageQueueServer.instance.broadcastMessage(new RemoveEntity(collider.id, RemoveEntity.Reasons.PLAYER_DIED, collidee.id));
            if (collidee.contains<PlayerInfo>())
            {
                collidee.get<PlayerInfo>().kills += 1;
            }
            mToRemove.Add(collider);
            var pos = collider.get<Position>();
            for (var segment = 0; segment < pos.segments.Count; segment++)
            {
                mToAdd.Add(createFood(false, pos.segments[segment]));
            }
        });

        mSysMovement = new Movement();

        mSysLifetime = new Shared.Systems.Lifetime(food =>
        {
            // Called when food naturally despawns
            mToRemove.Add(food);
            if (food.get<Shared.Components.Food>().naturalSpawn) mToAdd.Add(createFood());
            MessageQueueServer.instance.broadcastMessage(new RemoveEntity(food.id, RemoveEntity.Reasons.FOOD_EXPIRED, null));
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
            var message = new Shared.Messages.RemoveEntity(mClientToEntityId[clientId], RemoveEntity.Reasons.PLAYER_DISCONNECT, null);
            MessageQueueServer.instance.broadcastMessage(message);
            removeEntity(mClientToEntityId[clientId]);

            mClientToEntityId.Remove(clientId);
        }
    }

    private void handleRespawn(int clientId, string playerName)
    {
        // Check if player already has a snake
        if (mClientToEntityId.ContainsKey(clientId))
        {
            var oldId = mClientToEntityId[clientId];
            removeEntity(oldId);
            MessageQueueServer.instance.broadcastMessage(new RemoveEntity(oldId, RemoveEntity.Reasons.PLAYER_RESPAWNED, null));
        }
        spawnSnake(clientId, playerName);
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
        var snake = createSnake(playerName);
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
                MessageQueueServer.instance.broadcastMessage(new NewEntity(food));
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

    private Entity createSnake(string playerName)
    {
        var rng = new ExtendedRandom();
        bool done = false;
        var proposed = SnakeSegment.create("Images/Snake_Sheet", 0, 0, 0, playerName);

        while (!done)
        {
            int x = (int)rng.nextRange(1, Constants.ARENA_SIZE - 1);
            int y = (int)rng.nextRange(1, Constants.ARENA_SIZE - 1);
            var pos = proposed.get<Position>().segments[0];
            pos.X = x;
            pos.Y = y;
            proposed.get<Position>().segments[0] = pos;
            if (!mSysCollision.anyCollision(proposed))
            {
                proposed.get<Shared.Components.Movable>().segmentsToAdd = 3;
                return proposed;
            }
        }

        return null;
    }
}