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

        // if (movable.segmentsToAdd == 0 && pos.segments.Count > 0)
        // {
        //     pos.segments.RemoveAt(pos.segments.Count - 1);
        // }
        // else
        // {
        //     movable.segmentsToAdd -= 1;
        // }
        
        var xInc = (float)(Math.Cos(angle) * dist);
        var yInc = (float)(Math.Sin(angle) * dist);
        front.X += xInc;
        front.Y += yInc;
        pos.segments[0] = front;

        // TODO: Move the rest of the body and increase length

    }
}