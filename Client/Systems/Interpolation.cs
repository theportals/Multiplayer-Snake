using System;
using System.Collections.Generic;
using System.Numerics;
using Shared.Entities;

namespace Client.Systems;

public class Interpolation : Shared.Systems.System
{
    public Interpolation() : base(typeof(Shared.Components.Position), typeof(Shared.Components.Movable))
    {
    }

    public override bool add(Entity entity)
    {
        var interested = false;
        if (!entity.contains<Shared.Components.Input>())
        {
            if (base.add(entity))
            {
                interested = true;
                var position = entity.get<Shared.Components.Position>();
                var movement = entity.get<Shared.Components.Movable>();
                entity.add(new Components.Goal(position.segments, movement.facing));
            }
        }

        return interested;
    }
    
    public override void update(TimeSpan gameTime)
    {
        foreach (var entity in mEntities.Values)
        {
            var position = entity.get<Shared.Components.Position>();
            var movement = entity.get<Shared.Components.Movable>();
            var goal = entity.get<Components.Goal>();

            if (goal.updateWindow > TimeSpan.Zero && goal.updatedTime < goal.updateWindow)
            {
                goal.updatedTime += gameTime;
                var updateFraction = (float)gameTime.Milliseconds / goal.updateWindow.Milliseconds;

                movement.facing = movement.facing - (goal.startFacing - goal.goalFacing) * updateFraction;

                var startSegments = position.segments.Count;
                position.segments = new List<Vector2>();
                for (var i = 0; i < goal.goalSegments.Count; i++)
                {
                    if (i >= startSegments)
                    {
                        Console.WriteLine("i exceeded startSegments in Interpolation.cs");
                        position.segments.Add(goal.goalSegments[i]);
                        continue;
                    }
                    position.segments.Add(
                        new Vector2(
                            position.segments[i].X - (goal.startSegments[i].X - goal.goalSegments[i].X) * updateFraction,
                            position.segments[i].Y - (goal.startSegments[i].Y - goal.goalSegments[i].Y) * updateFraction));
                }
            }
        }
    }
}