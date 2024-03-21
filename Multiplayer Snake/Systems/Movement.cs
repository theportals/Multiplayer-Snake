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
        front.X += (int)(Math.Cos(angle) * dist);
        front.Y += (int)(Math.Sin(angle) * dist);
        
        // TODO: Move the rest of the body and increase length

    }
}