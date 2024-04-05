using System;
using Client.Components;
using Shared.Components;
using Vector2 = System.Numerics.Vector2;

namespace Client.Systems;

public class Movement : Shared.Systems.System
{
    private const float squiggleFactor = 10;
    private const float antiSquiggleFactor = 10;
    private const float segmentDistance = 10f / (squiggleFactor + antiSquiggleFactor);
    public Movement()
        : base(typeof(Shared.Components.Movable), typeof(Shared.Components.Position))
    {
        
    }
    
    public override void update(TimeSpan gameTime)
    {
        foreach (var entity in mEntities.Values)
        {
            moveEntity(entity, gameTime);
            if (entity.contains<RotationOffset>())
            {
                var rot = entity.get<RotationOffset>();
                rot.offset += (float)(rot.rotationSpeed * gameTime.TotalSeconds);
            }
        }
    }

    private void moveEntity(Shared.Entities.Entity entity, TimeSpan gameTime)
    {
        var movable = entity.get<Shared.Components.Movable>();
        var pos = entity.get<Shared.Components.Position>();
        var dist = gameTime.TotalSeconds * movable.moveSpeed;

        if (entity.contains<Boostable>())
        {
            var boost = entity.get<Boostable>();
            if (boost.boosting)
            {
                if (boost.stamina > 0)
                {
                    boost.stamina -= (float)gameTime.TotalSeconds;
                    dist *= boost.speedModifier;
                }
                else
                {
                    dist *= boost.penaltySpeed;
                }
            }
            else
            {
                if (boost.stamina < boost.maxStamina)
                {
                    var remaining = boost.maxStamina - boost.stamina;
                    boost.stamina += (float)Math.Min(remaining, gameTime.TotalSeconds * boost.regenRate);
                }
            }
        }

        var front = pos.segments[0];
        var angle = movable.facing;
        
        // const float maxTurn = (float)Math.PI / 32;
        var maxTurn = movable.turnSpeed * gameTime.TotalSeconds;
        while (movable.segmentsToAdd > 0)
        {
            var tail = pos.segments[^1];
            var spawnDir = movable.facing - Math.PI;
            if (pos.segments.Count > 1) spawnDir = Math.Atan2(tail.Y - pos.segments[^2].Y, tail.X - pos.segments[^2].X);
            pos.segments.Add(new Vector2((float)(tail.X + Math.Cos(spawnDir) * segmentDistance * (squiggleFactor + antiSquiggleFactor)), (float)(tail.Y + Math.Sin(spawnDir) * segmentDistance)));
            movable.segmentsToAdd -= 1;
        }
        
        var xInc = (float)(Math.Cos(angle) * dist);
        var yInc = (float)(Math.Sin(angle) * dist);
        front.X += xInc;
        front.Y += yInc;
        pos.segments[0] = front;

        for (int i = 1; i < pos.segments.Count; i++)
        {
            var leader = pos.segments[i - 1];
            var segment = pos.segments[i];
            var dir = Math.Atan2(leader.Y - segment.Y, leader.X - segment.X);
            var needx = (float)(leader.X - Math.Cos(dir) * segmentDistance * squiggleFactor);
            var needy = (float)(leader.Y - Math.Sin(dir) * segmentDistance * squiggleFactor);
            
            if (i > 1)
            {
                var dir2 = Math.Atan2(pos.segments[i - 2].Y - leader.Y, pos.segments[i - 2].X - leader.X);
                if (Math.Abs(dir2 - dir) > maxTurn)
                {
                    var dl = dir2 - dir;
                    if (dl < 0) dl += 2 * Math.PI;
                    var dr = dir - dir2;
                    if (dr < 0) dr += 2 * Math.PI;
                    var sign = -1;
                    if (dl < dr) sign = 1;
                    
                    var correction = dir2 - maxTurn * sign;
                    needx = (float)(leader.X - Math.Cos(correction) * segmentDistance * squiggleFactor);
                    needy = (float)(leader.Y - Math.Sin(correction) * segmentDistance * squiggleFactor);
                    dir = Math.Atan2(needy - segment.Y, needx - segment.X);
                }
            }
            var d = Math.Sqrt(Math.Pow(needx - segment.X, 2) + Math.Pow(needy - segment.Y, 2));

            if (d >= segmentDistance * antiSquiggleFactor)
            {
                xInc = (float)(Math.Cos(dir) * (d - segmentDistance * antiSquiggleFactor));
                yInc = (float)(Math.Sin(dir) * (d - segmentDistance * antiSquiggleFactor));
                segment.X += xInc;
                segment.Y += yInc;
                pos.segments[i] = segment;
            }
        }

    }
}