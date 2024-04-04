using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Entities;

namespace Client.Systems;

public class Lifetime : Shared.Systems.System
{
    private Action<Entity> mOnExpire;

    public Lifetime(Action<Entity> onExpire)
        : base(typeof(Components.Lifetime))
    {
        mOnExpire = onExpire;
    }

    public override void update(TimeSpan gameTime)
    {
        foreach (var entity in mEntities.Values)
        {
            var lifetime = entity.get<Components.Lifetime>();
            lifetime.timeAlive += (float)gameTime.TotalSeconds;
            if (lifetime.timeAlive >= lifetime.lifetime)
            {
                mOnExpire.Invoke(entity);
                mEntities.Remove(entity.id);
            }
        }
    }
}