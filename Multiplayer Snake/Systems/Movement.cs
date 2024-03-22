using System;
using Microsoft.Xna.Framework;
using NotImplementedException = System.NotImplementedException;

namespace Multiplayer_Snake.Systems;

public class Movement : System
{
    private const float squiggleFactor = 10;
    private const float antiSquiggleFactor = 10;
    private const float segmentDistance = 10f / (squiggleFactor + antiSquiggleFactor);
    public Movement()
        : base(typeof(Components.Movable), typeof(Components.Position))
    {
        
    }
    
    public override void Update(GameTime gameTime)
    {
        foreach (var entity in mEntities.Values)
        {
            moveEntity(entity, gameTime);
        }
    }

    private void moveEntity(Entities.Entity entity, GameTime gameTime)
    {
        var movable = entity.GetComponent<Components.Movable>();
        var pos = entity.GetComponent<Components.Position>();
        var dist = gameTime.ElapsedGameTime.TotalSeconds * movable.moveSpeed;

        var front = pos.segments[0];
        var angle = movable.facing;
        
        // const float maxTurn = (float)Math.PI / 32;
        var maxTurn = movable.turnSpeed * gameTime.ElapsedGameTime.TotalSeconds;
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