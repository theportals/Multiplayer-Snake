using Shared.Entities;

namespace Shared.Systems;

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
        return ComponentTypes.All(entity.contains);
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

    public abstract void Update(TimeSpan gameTime);
}