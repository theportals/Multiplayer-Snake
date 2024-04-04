using Client.Entities;
using Shared.Entities;
using Shared.Messages;

namespace Server;

public class GameModel
{
    private HashSet<int> mClients = new HashSet<int>();
    private Dictionary<uint, Entity> mEntities = new Dictionary<uint, Entity>();
    private Dictionary<int, uint> mClientToEntityId = new Dictionary<int, uint>();

    Systems.Network mSystemNetwork = new Server.Systems.Network();

    /// <summary>
    /// This is where the server-side simulation takes place.  Messages
    /// from the network are processed and then any necessary client
    /// updates are sent out.
    /// </summary>
    public void update(TimeSpan elapsedTime)
    {
        mSystemNetwork.update(elapsedTime, MessageQueueServer.instance.getMessages());
    }

    public bool initialize()
    {
        mSystemNetwork.registerJoinHandler(handleJoin);
        mSystemNetwork.registerDisconnectHandler(handleDisconnect);
        
        MessageQueueServer.instance.registerConnectHandler(handleConnect);
        
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

        var message = new Shared.Messages.RemoveEntity(mClientToEntityId[clientId]);
        MessageQueueServer.instance.broadcastMessage(message);

        removeEntity(mClientToEntityId[clientId]);

        mClientToEntityId.Remove(clientId);
    }

    private void addEntity(Entity entity)
    {
        if (entity == null)
        {
            return;
        }

        mEntities[entity.Id] = entity;
        mSystemNetwork.Add(entity);
    }

    private void removeEntity(uint id)
    {
        mEntities.Remove(id);
        mSystemNetwork.Remove(id);
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
        
        // Step 2: Create an entity for the newly joined player and send it to the newly joined client
        // TODO: Player spawns in the least populated area
        var player = SnakeSegment.create("Images/Snake_Sheet", 500, 500);
        addEntity(player);
        mClientToEntityId[clientId] = player.Id;
        
        // Step 3: Send the new player entity to the newly joined client
        MessageQueueServer.instance.sendMessage(clientId, new NewEntity(player));
        
        // Step 4: Let all other clients know about the new entity
        
        // Remove components not needed for other players
        player.remove<Shared.Components.Input>();

        var message = new NewEntity(player);
        foreach (int otherId in mClients)
        {
            if (otherId != clientId)
            {
                MessageQueueServer.instance.sendMessage(otherId, message);
            }
        }
    }
}