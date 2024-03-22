using System;
using Microsoft.Xna.Framework;
using NotImplementedException = System.NotImplementedException;

namespace Multiplayer_Snake.Systems;

public class Movement : System
{
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

        const int segmentDistance = 10;
        while (movable.segmentsToAdd > 0)
        {
            var tail = pos.segments[^1];
            var spawnDir = movable.facing - Math.PI;
            if (pos.segments.Count > 1) spawnDir = Math.Atan2(tail.Y - pos.segments[^2].Y, tail.X - pos.segments[^2].X);
            pos.segments.Add(new Vector2((float)(tail.X + Math.Cos(spawnDir) * segmentDistance), (float)(tail.Y + Math.Sin(spawnDir) * segmentDistance)));
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
            var d = Math.Sqrt(Math.Pow(leader.X - segment.X, 2) + Math.Pow(leader.Y - segment.Y, 2));
            if (d > segmentDistance)
            {
                xInc = (float)(Math.Cos(dir) * (d - segmentDistance));
                yInc = (float)(Math.Sin(dir) * (d - segmentDistance));
                segment.X += xInc;
                segment.Y += yInc;
                pos.segments[i] = segment;
            }
        }

    }
}