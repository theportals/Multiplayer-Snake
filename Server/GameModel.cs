using Shared.Entities;

namespace Server;

public class GameModel
{
    private HashSet<int> m_clients = new HashSet<int>();
    private Dictionary<uint, Entity> m_entities = new Dictionary<uint, Entity>();
    private Dictionary<int, uint> m_clientToEntityId = new Dictionary<int, uint>();

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
        // TODO: Server game model
        Console.WriteLine("GameModel.initialize() not implemented");
        return false;
    }

    public void shutdown()
    {
        
    }
}