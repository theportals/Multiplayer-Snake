using System;
using Microsoft.Xna.Framework;

namespace Client.Particles;

public class Particle
{
    
    public long name;
    public Vector2 size;
    public Vector2 loc;
    public float rotation;
    private Vector2 direction;
    private float speed;
    private TimeSpan lifetime;
    private TimeSpan alive = TimeSpan.Zero;
    private static long m_nextName = 0;
    private float rotationFactor;
    public Particle(Vector2 loc, Vector2 direction, float speed, Vector2 size, TimeSpan lifetime, float rotationFactor)
    {
        this.name = m_nextName++;
        this.loc = loc;
        this.direction = direction;
        this.speed = speed;
        this.size = size;
        this.lifetime = lifetime;
        this.rotationFactor = rotationFactor;

        this.rotation = 0;
    }

    public bool update(GameTime gameTime)
    {
        // Update how long it has been alive
        alive += gameTime.ElapsedGameTime;

        // Update its center
        loc.X += (float)(gameTime.ElapsedGameTime.TotalMilliseconds * speed * direction.X);
        loc.Y += (float)(gameTime.ElapsedGameTime.TotalMilliseconds * speed * direction.Y);

        // Rotate proportional to its speed
        rotation += (speed * rotationFactor);

        // Return true if this particle is still alive
        return alive < lifetime;
    }
}