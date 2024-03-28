using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;
using Shared.Entities;

namespace Client.Util;

public class ParticleUtil
{
    public static List<Entity> playerDeath(Texture2D fire, Texture2D smoke, Entity ss)
    {
        var rng = new ExtendedRandom();
        var particles = new List<Entity>();
        var pos = ss.get<Position>();
        
        for (var segment = 0; segment < pos.segments.Count; segment++)
        {
            for (int i = 0; i < 5; i++)
            {
                particles.Add(fireParticle(fire, pos.segments[segment].X, pos.segments[segment].Y, rng));
                particles.Add(smokeParticle(smoke, pos.segments[segment].X, pos.segments[segment].Y, rng));
            }
        }
        return particles;
    }

    public static void enemyDeath(Texture2D fire, Entity ss)
    {
        // TODO: Enemy death particles
    }

    public static Entity fireParticle(Texture2D fire, float x, float y, ExtendedRandom rng)
    {
        return Entities.Particle.create(fire, 
            x, y, 
            rng, 
            10, 4, 
            (float)rng.nextCircleAngle(), 0f, 
            120f, 50f, 
            0.5f,0.05f, 
            0f, 
            5f, 5f);
    }

    public static Entity smokeParticle(Texture2D smoke, float x, float y, ExtendedRandom rng)
    {
        return Entities.Particle.create(smoke, 
            x, y, 
            rng, 
            15, 4, 
            (float)rng.nextCircleAngle(), 0f, 
            120f, 50f, 
            3f,1f, 
            0f, 
            5f, 5f);
    }
}