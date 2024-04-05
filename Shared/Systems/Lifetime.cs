using Shared.Entities;

namespace Shared.Systems;

public class Lifetime : Shared.Systems.System
{
    private Action<Entity> mOnExpire;

    public Lifetime(Action<Entity> onExpire)
        : base(typeof(Shared.Components.Lifetime))
    {
        mOnExpire = onExpire;
    }

    public override void update(TimeSpan gameTime)
    {
        foreach (var entity in mEntities.Values)
        {
            var lifetime = entity.get<Shared.Components.Lifetime>();
            lifetime.timeAlive += (float)gameTime.TotalSeconds;
            if (lifetime.timeAlive >= lifetime.lifetime)
            {
                mOnExpire.Invoke(entity);
                mEntities.Remove(entity.id);
            }
        }
    }
}