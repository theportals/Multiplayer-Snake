using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Multiplayer_Snake.Entities;

namespace Multiplayer_Snake.Systems;

public abstract class System
{
    protected Dictionary<uint, Entity> mEntities = new();
    
    private Type[] ComponentTypes { get; set; }

    public System(params Type[] componentTypes)
    {
        ComponentTypes = componentTypes;
    }

    protected virtual bool isInterested(Entity entity)
    {
        return ComponentTypes.All(entity.ContainsComponent);
    }

    public bool Add(Entity entity)
    {
        var interested = isInterested(entity);
        if (interested)
        {
            mEntities.Add(entity.Id, entity);
        }

        return interested;
    }

    public bool Remove(uint id)
    {
        return mEntities.Remove(id);
    }

    public abstract void Update(GameTime gameTime);
}