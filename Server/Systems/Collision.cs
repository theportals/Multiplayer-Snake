using Shared.Components;
using Shared.Entities;
using Food = Shared.Components.Food;

namespace Server.Systems;

public class Collision : Shared.Systems.System
{
    private Action<Entity, Entity> mFoodConsumed;
    private Action<Entity, Entity> mOnCollision;

    public Collision(Action<Entity, Entity> foodConsumed, Action<Entity, Entity> onCollision) 
        : base(typeof(Shared.Components.Position))
    {
        mFoodConsumed = foodConsumed;
        mOnCollision = onCollision;
    }
    
    public override void update(TimeSpan gameTime)
    {
        var movable = findMovable(mEntities);

        foreach (var entityA in mEntities.Values)
        {
            var colA = entityA.get<Shared.Components.Collision>();
            if (colA.intangibility > 0)
            {
                colA.intangibility -= (float)gameTime.TotalSeconds;
                if (colA.intangibility < 0) colA.intangibility = 0;
            }
            
            foreach (var entityB in movable)
            {
                var colB = entityB.get<Shared.Components.Collision>();
                if (collides(entityA, entityB))
                {
                    // No worries if collides with food
                    if (entityA.contains<Food>())
                    {
                        var food = entityA.get<Food>();
                        if (food.naturalSpawn) entityB.get<Shared.Components.Movable>().segmentsToAdd += 3;
                        else entityB.get<Shared.Components.Movable>().segmentsToAdd += 1;
                        mFoodConsumed(entityA, entityB);
                    }
                    // Always collide with border block, regardless of intangibility
                    else if (entityA.contains<Border>())
                    {
                        mOnCollision(entityB, entityA);
                    }
                    else
                    {
                        if (colA.intangibility <= 0 && colB.intangibility <= 0) mOnCollision(entityB, entityA);
                    }
                }
            }
        }
    }

    public bool anyCollision(Entity proposed)
    {
        return mEntities.Values
            .Where(entity => entity.contains<Shared.Components.Collision>() 
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
        var aCol = a.get<Shared.Components.Collision>();
        var bPos = b.get<Shared.Components.Position>();
        var bCol = b.get<Shared.Components.Collision>();

        float d2;
        double r2;
        if (a == b)
        {
            for (int segment = 1; segment < aPos.segments.Count; segment++)
            {
                d2 = (aPos.segments[segment].X - aPos.segments[0].X) * (aPos.segments[segment].X - aPos.segments[0].X) + (aPos.segments[segment].Y - aPos.segments[0].Y) * (aPos.segments[segment].Y - aPos.segments[0].Y);
                if (d2 < aCol.size)
                {
                    return true;
                }
            }

            return false;
        }

        if (bPos.segments.Count == 0)
        {
            d2 = (bPos.x - aPos.x) * (bPos.x - aPos.x) + (bPos.y - aPos.y) * (bPos.y - aPos.y);
            r2 = Math.Pow(Math.Max(aCol.size, bCol.size), 2);
            return d2 <= r2;
        }
         
        for (int segment = 0; segment < aPos.segments.Count; segment++)
        {
            d2 = (bPos.x - aPos.segments[segment].X) * (bPos.x - aPos.segments[segment].X) +
                 (bPos.y - aPos.segments[segment].Y) * (bPos.y - aPos.segments[segment].Y);
            if (d2 <= Math.Pow(Math.Max(aCol.size, bCol.size), 2)) return true;
        }

        return false;
    }
}