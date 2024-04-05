using System;
using System.Collections.Generic;
using Shared.Components;
using Shared.Messages;
using Type = Shared.Messages.Type;

namespace Client.Systems;

public class Network : Shared.Systems.System
{
    public delegate void Handler(TimeSpan elapsedTime, Message message);
    public delegate void RemoveEntityHandler(RemoveEntity message);
    public delegate void NewEntityHandler(NewEntity message);

    private Dictionary<Type, Handler> mCommandMap = new();
    private RemoveEntityHandler mRemoveEntityHandler;
    private NewEntityHandler mNewEntityHandler;
    private uint mLastMessageId = 0;
    private HashSet<uint> mUpdatedEntries = new();

    public Network() : base(typeof(Position))
    {
        registerHandler(Type.ConnectAck, (gameTime, message) =>
        {
            handleConnectAck(gameTime, (ConnectAck)message);
        });
        
        registerHandler(Type.NewEntity, (gameTime, message) =>
        {
            mNewEntityHandler((NewEntity)message);
        });
        
        registerHandler(Type.UpdateEntity, (gameTime, message) =>
        {
            handleUpdateEntity(gameTime, (UpdateEntity)message);
        });
        
        registerHandler(Type.RemoveEntity, (gameTime, message) =>
        {
            mRemoveEntityHandler((RemoveEntity)message);
        });
    }
    
    public override void update(TimeSpan gameTime)
    {
    }

    public void update(TimeSpan gameTime, Queue<Message> messages)
    {
        mUpdatedEntries.Clear();

        if (messages != null)
        {
            while (messages.Count > 0)
            {
                var message = messages.Dequeue();
                if (mCommandMap.ContainsKey(message.type))
                {
                    mCommandMap[message.type](gameTime, message);
                }

                if (message.messageId.HasValue)
                {
                    mLastMessageId = message.messageId.Value;
                }
            }
        }
        
        // After processing all messages, perform reconciliation
        var sent = MessageQueueClient.instance.getSendMessageHistory(mLastMessageId);
        while (sent.Count > 0)
        {
            var message = (Shared.Messages.Input)sent.Dequeue();
            if (message.type == Type.Input)
            {
                var entity = mEntities[message.entityId];
                if (mUpdatedEntries.Contains(entity.id))
                {
                    foreach (var input in message.inputs)
                    {
                        switch (input)
                        {
                            //TODO: Simulate input messages
                            case Shared.Components.Input.Type.BOOST:
                                break;
                            case Shared.Components.Input.Type.TURN_LEFT:
                                break;
                            case Shared.Components.Input.Type.TURN_RIGHT:
                                break;
                        }
                    }
                }
            }
        }
    }

    private void registerHandler(Type type, Handler handler)
    {
        mCommandMap[type] = handler;
    }

    public void registerNewEntityHandler(NewEntityHandler handler)
    {
        mNewEntityHandler = handler;
    }

    public void registerRemoveEntityHandler(RemoveEntityHandler handler)
    {
        mRemoveEntityHandler = handler;
    }

    private void handleConnectAck(TimeSpan gameTime, ConnectAck message)
    {
        Console.WriteLine("Sending Join");
        MessageQueueClient.instance.sendMessage(new Join(Client.playerName));
    }

    private void handleUpdateEntity(TimeSpan gameTime, UpdateEntity message)
    {
        if (mEntities.ContainsKey(message.id))
        {
            var entity = mEntities[message.id];
            if (entity.contains<Components.Goal>() && message.hasPosition && message.hasMovement)
            {
                var position = entity.get<Position>();
                var movement = entity.get<Movable>();
                var goal = entity.get<Components.Goal>();

                goal.updateWindow = message.updateWindow;
                goal.updatedTime = TimeSpan.Zero;
                goal.goalSegments = message.segments;
                goal.goalFacing = message.facing;

                goal.startSegments = position.segments;
                goal.startFacing = movement.facing;
            }
            else if (entity.contains<Position>() && message.hasPosition && entity.contains<Movable>() &&
                     message.hasMovement)
            {
                entity.get<Position>().segments = message.segments;
                entity.get<Movable>().facing = message.facing;

                mUpdatedEntries.Add(entity.id);
            }
        }
    }
}