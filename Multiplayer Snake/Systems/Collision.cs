using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Microsoft.Xna.Framework;
using Multiplayer_Snake.Entities;
using NotImplementedException = System.NotImplementedException;

namespace Multiplayer_Snake.Systems;

public class Collision : System
{
    private Action<Entity> mFoodConsumed;
    private Action<Entity> mOnCollision;

    public Collision(Action<Entity> foodConsumed, Action<Entity> onCollision) 
        : base(typeof(Components.Position))
    {
        mFoodConsumed = foodConsumed;
        mOnCollision = onCollision;
    }
    
    public override void Update(GameTime gameTime)
    {
        var movable = findMovable(mEntities);

        foreach (var entity in mEntities.Values)
        {
            foreach (var entityMovable in movable)
            {
                if (collides(entity, entityMovable))
                {
                    // No worries if collides with food
                    if (entity.ContainsComponent<Components.Food>())
                    {
                        entityMovable.GetComponent<Components.Movable>().segmentsToAdd = 3;
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
            .Where(entity => entity.ContainsComponent<Components.Collision>() 
                             && entity.ContainsComponent<Components.Position>())
            .Any(entity => collides(proposed, entity));
    }

    private List<Entity> findMovable(Dictionary<uint, Entity> entities)
    {
        return mEntities.Values.Where(entity => entity.ContainsComponent<Components.Movable>() && entity.ContainsComponent<Components.Position>()).ToList();
    }

    private bool collides(Entity a, Entity b)
    {
        var aPos = a.GetComponent<Components.Position>();
        var aCol = a.GetComponent<Components.Collision>();
        var bPos = b.GetComponent<Components.Position>();
        var bCol = b.GetComponent<Components.Collision>();

        if (a == b) return false;   // We don't care if a player collides with themself

        var d2 = (bPos.x - aPos.x) * (bPos.x - aPos.x) + (bPos.y - aPos.y) * (bPos.y - aPos.y);
        var r2 = Math.Pow(Math.Max(aCol.size, bCol.size), 2);
        return d2 <= r2;
    }
}