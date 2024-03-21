using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Multiplayer_Snake.Components;

namespace Multiplayer_Snake.Entities;

public sealed class Entity
{
    private readonly Dictionary<Type, Component> components = new();

    private static uint mNextId = 0;
    
    public uint Id { get; private set; }

    public Entity()
    {
        Id = mNextId++;
    }

    public bool ContainsComponent(Type type)
    {
        return components.ContainsKey(type) && components[type] != null;
    }

    public bool ContainsComponent<TComponent>()
        where TComponent : Component
    {
        return ContainsComponent(typeof(TComponent));
    }

    public Entity Add(Component component)
    {
        Debug.Assert(component != null, "component cannot be null");
        Debug.Assert(!components.ContainsKey(component.GetType()), "cannot add the same component twice");
        
        components.Add(component.GetType(), component);
        return this;
    }

    public void Clear()
    {
        components.Clear();
    }

    public void Remove<TComponent>()
        where TComponent : Component
    {
        components.Remove(typeof(TComponent));
    }

    public TComponent GetComponent<TComponent>()
        where TComponent : Component
    {
        Debug.Assert(components.ContainsKey(typeof(TComponent)), $"component of type {typeof(TComponent)} is not a part of this entity");
        return (TComponent)components[typeof(TComponent)];
    }

    public override string ToString()
    {
        return $"{Id}: {string.Join(", ", from c in components.Values select c.GetType().Name)}";
    }
}