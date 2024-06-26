using System;
using System.Collections.Generic;
using Shared.Components;
using Shared.Messages;
using Shared.Systems;
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

    private Dictionary<uint, uint> mServerIdToClientId = new();

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

    public void mapServerToClientId(uint serverId, uint clientId)
    {
        mServerIdToClientId[serverId] = clientId;
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
            var dq = sent.Dequeue();
            if (dq.type == Type.Input)
            {
                var message = (Shared.Messages.Input)dq;
                if (!mServerIdToClientId.ContainsKey(message.entityId) 
                    || !mEntities.ContainsKey(mServerIdToClientId[message.entityId])) continue;
                var entity = mEntities[mServerIdToClientId[message.entityId]];
                if (mUpdatedEntries.Contains(entity.id))
                {
                    Movement.moveEntity(entity, message.elapsedTime);
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
        MessageQueueClient.instance.sendMessageWithId(new Join(Client.playerName));
    }

    private void handleUpdateEntity(TimeSpan gameTime, UpdateEntity message)
    {
        if (!mServerIdToClientId.ContainsKey(message.id) 
            || !mEntities.ContainsKey(mServerIdToClientId[message.id])) return;
        var entity = mEntities[mServerIdToClientId[message.id]];
        if (entity.contains<Components.Goal>())
        {
            var goal = entity.get<Components.Goal>();
            if (message.hasPosition)
            {
                var position = entity.get<Position>();
                goal.startSegments = position.segments;
            }

            if (message.hasMovement)
            {
                var movement = entity.get<Movable>();
                goal.startFacing = movement.facing;
            }

            if (message.hasBoost)
            {
                var boost = entity.get<Boostable>();
                goal.startStamina = boost.stamina;
            }

            if (message.hasPlayerInfo)
            {
                var info = entity.get<PlayerInfo>();
                goal.startScore = info.score;
                goal.startKills = info.kills;
            }

            if (message.hasCollision)
            {
                var collision = entity.get<Collision>();
                goal.startCollisionSize = collision.size;
                goal.startIntangibility = collision.intangibility;
            }

            goal.updateWindow = message.updateWindow;
            goal.updatedTime = TimeSpan.Zero;
            goal.goalSegments = message.segments;
            goal.goalFacing = message.facing;
            goal.goalStamina = message.stamina;
            goal.goalScore = message.score;
            goal.goalKills = message.kills;
            goal.goalCollisionSize = message.collisionSize;
            goal.goalIntangibility = message.intangibility;
        }
        else if (entity.contains<Position>() && message.hasPosition 
                                             && entity.contains<Movable>() && message.hasMovement 
                                             && entity.contains<PlayerInfo>() && message.hasPlayerInfo)
        {
            entity.get<Position>().segments = message.segments;
            entity.get<Movable>().facing = message.facing;
            entity.get<Boostable>().stamina = message.stamina;
            entity.get<PlayerInfo>().score = message.score;
            entity.get<PlayerInfo>().kills = message.kills;
            entity.get<Collision>().size = message.collisionSize;
            entity.get<Collision>().intangibility = message.intangibility;

            mUpdatedEntries.Add(entity.id);
        }
    }
}