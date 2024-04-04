using System.Diagnostics;
using Shared.Components;

namespace Shared.Entities;

public sealed class Entity
{
    private readonly Dictionary<Type, Component> components = new();

    private static uint mNextId = 0;
    
    public uint id { get; private set; }

    public Entity()
    {
        id = mNextId;
        mNextId += 1;
        if (id == 0)
        {
            Console.WriteLine("Ruh roh!");
        }
    }

    public Entity(uint id)
    {
        mNextId = id + 1;
        this.id = id;
        if (id == 0)
        {
            Console.WriteLine("Ruh roh!");
        }
    }

    public bool contains(Type type)
    {
        return components.ContainsKey(type) && components[type] != null;
    }

    public bool contains<TComponent>()
        where TComponent : Component
    {
        return contains(typeof(TComponent));
    }

    public Entity add(Component component)
    {
        Debug.Assert(component != null, "component cannot be null");
        Debug.Assert(!components.ContainsKey(component.GetType()), "cannot add the same component twice");
        
        components.Add(component.GetType(), component);
        return this;
    }

    public void clear()
    {
        components.Clear();
    }

    public void remove<TComponent>()
        where TComponent : Component
    {
        components.Remove(typeof(TComponent));
    }

    public TComponent get<TComponent>()
        where TComponent : Component
    {
        Debug.Assert(components.ContainsKey(typeof(TComponent)), $"component of type {typeof(TComponent)} is not a part of this entity");
        return (TComponent)components[typeof(TComponent)];
    }

    public override string ToString()
    {
        return $"{id}: {string.Join(", ", from c in components.Values select c.GetType().Name)}";
    }
}