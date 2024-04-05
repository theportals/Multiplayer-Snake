using System.Numerics;
using Server.Systems;
using Shared.Entities;
using Shared.Messages;
using Shared.Util;

namespace Server;

public class GameModel
{
    
    private HashSet<int> mClients = new HashSet<int>();
    private Dictionary<uint, Entity> mEntities = new Dictionary<uint, Entity>();
    private Dictionary<int, uint> mClientToEntityId = new Dictionary<int, uint>();

    private List<Entity> mToRemove = new();

    Systems.Network mSystemNetwork = new Server.Systems.Network();
    private Collision mSysCollision;

    /// <summary>
    /// This is where the server-side simulation takes place.  Messages
    /// from the network are processed and then any necessary client
    /// updates are sent out.
    /// </summary>
    public void update(TimeSpan elapsedTime)
    {
        mSystemNetwork.update(elapsedTime, MessageQueueServer.instance.getMessages());
        
        mSysCollision.update(elapsedTime);
    }

    public bool initialize()
    {
        mSystemNetwork.registerJoinHandler(handleJoin);
        mSystemNetwork.registerDisconnectHandler(handleDisconnect);
        
        MessageQueueServer.instance.registerConnectHandler(handleConnect);

        mSysCollision = new Collision(food =>
        {
            // Called when player collides with food
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
            // mPlayerSnake = null;
            // mPause.gameOver = true;
            // mPause.open();
            // playerDeath(e);
            // addParticlesLater(ParticleUtil.playerDeath(fire, smoke, e));
            // thrustInstance.Pause();
            // explode.Play();
        });
        
        // Initialize arena
        initializeBorder();
        // initializeObstacles();
        // for (var i = 0; i < Constants.FOOD_COUNT; i++)
        // {
        //     addEntity(createFood());
        // }
        
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
    }

    private void removeEntity(uint id)
    {
        Console.WriteLine($"Removing entity {id}");
        mEntities.Remove(id);
        mSystemNetwork.remove(id);
        mSysCollision.remove(id);
    }

    private void reportAllEntities(int clientId)
    {
        foreach (var item in mEntities)
        {
            MessageQueueServer.instance.sendMessage(clientId, new Shared.Messages.NewEntity(item.Value));
        }
    }

    private void handleJoin(int clientId)
    {
        // Step 1: Tell the newly connected player about all other entities
        reportAllEntities(clientId);
        
        // // Step 2: Create an entity for the newly joined player and send it to the newly joined client
        // // TODO: Player spawns in the least populated area
        // var player = SnakeSegment.create("Images/Snake_Sheet", 500, 500, 3);
        // addEntity(player);
        // mClientToEntityId[clientId] = player.id;
        
        // // Step 3: Send the new player entity to the newly joined client
        // //TODO: Game currently crashes due to multiple entities with id 0 being added to the client. fix this.
        // MessageQueueServer.instance.sendMessage(clientId, new NewEntity(player));
        //
        // // Step 4: Let all other clients know about the new entity
        //
        // // Remove components not needed for other players
        // player.remove<Shared.Components.Input>();
        //
        // var message = new NewEntity(player);
        // foreach (int otherId in mClients)
        // {
        //     if (otherId != clientId)
        //     {
        //         MessageQueueServer.instance.sendMessage(otherId, message);
        //     }
        // }
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