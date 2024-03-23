using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Entities;

namespace Multiplayer_Snake.Systems;

public class Collision : Shared.Systems.System
{
    private Action<Entity> mFoodConsumed;
    private Action<Entity> mOnCollision;

    public Collision(Action<Entity> foodConsumed, Action<Entity> onCollision) 
        : base(typeof(Shared.Components.Position))
    {
        mFoodConsumed = foodConsumed;
        mOnCollision = onCollision;
    }
    
    public override void Update(TimeSpan gameTime)
    {
        var movable = findMovable(mEntities);

        foreach (var entity in mEntities.Values)
        {
            foreach (var entityMovable in movable)
            {
                if (collides(entity, entityMovable))
                {
                    // No worries if collides with food
                    if (entity.contains<Components.Food>())
                    {
                        entityMovable.get<Shared.Components.Movable>().segmentsToAdd = 3;
                        mFoodConsumed(entity);
                    }
                    else
                    {
                        mOnCollision(entityMovable);
                    }
                }
            }
        }
    }

    public bool anyCollision(Entity proposed)
    {
        return mEntities.Values
            .Where(entity => entity.contains<Components.Collision>() 
                             && entity.contains<Shared.Components.Position>())
            .Any(entity => collides(proposed, entity));
    }

    private List<Entity> findMovable(Dictionary<uint, Entity> entities)
    {
        return mEntities.Values.Where(entity => entity.contains<Shared.Components.Movable>() && entity.contains<Shared.Components.Position>()).ToList();
    }

    private bool collides(Entity a, Entity b)
    {
        var aPos = a.get<Shared.Components.Position>();
        var aCol = a.get<Components.Collision>();
        var bPos = b.get<Shared.Components.Position>();
        var bCol = b.get<Components.Collision>();

        if (a == b) return false;   // We don't care if a player collides with themself

        var d2 = (bPos.x - aPos.x) * (bPos.x - aPos.x) + (bPos.y - aPos.y) * (bPos.y - aPos.y);
        var r2 = Math.Pow(Math.Max(aCol.size, bCol.size), 2);
        return d2 <= r2;
    }
}