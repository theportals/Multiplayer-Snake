using System.Numerics;
using Shared.Components;
using Shared.Entities;
using Shared.Messages;

namespace Server.Systems;
public class Network : Shared.Systems.System
{
    public delegate void Handler(int clientId, TimeSpan elapsedTime, Message message);
    public delegate void JoinHandler(int clientId, string playerName);
    public delegate void DisconnectHandler(int clientId);
    public delegate void InputHandler(Entity entity, TimeSpan elapsedTime);

    private Dictionary<Shared.Messages.Type, Handler> mCommandMap = new();
    private JoinHandler mJoinHandler;
    private DisconnectHandler mDisconnectHandler;

    private HashSet<uint> mReportThese = new();

    /// <summary>
    /// Primary activity in the constructor is to setup the command map
    /// that maps from message types to their handlers.
    /// </summary>
    public Network() :
        base(
            typeof(Shared.Components.Movable),
            typeof(Shared.Components.Position)
        )
    {
        // Register our own join handler
        registerHandler(Shared.Messages.Type.Join, (int clientId, TimeSpan elapsedTime, Shared.Messages.Message message) =>
        {
            if (mJoinHandler != null)
            {
                mJoinHandler(clientId, ((Join)message).playerName);
            }
        });

        // Register our own disconnect handler
        registerHandler(Shared.Messages.Type.Disconnect, (int clientId, TimeSpan elapsedTime, Shared.Messages.Message message) =>
        {
            if (mDisconnectHandler != null)
            {
                mDisconnectHandler(clientId);
            }
        });

        // Register our own input handler
        registerHandler(Shared.Messages.Type.Input, (int clientId, TimeSpan elapsedTime, Shared.Messages.Message message) =>
        {
            handleInput((Shared.Messages.Input)message);
        });
    }

    // Have to implement this because it is abstract in the base class
    public override void update(TimeSpan elapsedTime) { }

    /// <summary>
    /// Have our own version of update, because we need a list of messages to work with, and
    /// messages aren't entities.
    /// </summary>
    public void update(TimeSpan elapsedTime, Queue<Tuple<int, Message>> messages)
    {
        if (messages != null)
        {
            while (messages.Count > 0)
            {
                var message = messages.Dequeue();
                if (mCommandMap.ContainsKey(message.Item2.type))
                {
                    mCommandMap[message.Item2.type](message.Item1, elapsedTime, message.Item2);
                }
            }
        }

        // Send updated game state updates back out to connected clients
        updateClients(elapsedTime);
    }

    public void registerJoinHandler(JoinHandler handler)
    {
        mJoinHandler = handler;
    }

    public void registerDisconnectHandler(DisconnectHandler handler)
    {
        mDisconnectHandler = handler;
    }

    private void registerHandler(Shared.Messages.Type type, Handler handler)
    {
        mCommandMap[type] = handler;
    }

    /// <summary>
    /// Handler for the Input message.  This simply passes the responsibility
    /// to the registered input handler.
    /// </summary>
    /// <param name="message"></param>
    private void handleInput(Shared.Messages.Input message)
    {
        var entity = mEntities[message.entityId];
        var pos = entity.get<Position>();
        var movable = entity.get<Movable>();
        var boost = entity.get<Boostable>();

        movable.facing = message.newFacing;
        boost.boosting = message.boosting;
        // mReportThese.Add(entity.id);
    }

    /// <summary>
    /// For the entities that have updates, send those updates to all
    /// connected clients.
    /// </summary>
    private void updateClients(TimeSpan elapsedTime)
    {
        // foreach (uint entityId in mReportThese)
        // {
        //     var entity = mEntities[entityId];
        //     var message = new Shared.Messages.UpdateEntity(entity, elapsedTime);
        //     MessageQueueServer.instance.broadcastMessageWithLastId(message);
        // }
        //
        // mReportThese.Clear();
        foreach (var entry in mEntities)
        {
            var entity = entry.Value;
            var message = new Shared.Messages.UpdateEntity(entity, elapsedTime);
            MessageQueueServer.instance.broadcastMessageWithLastId(message);
        }

        // mReportThese.Clear();
    }
}