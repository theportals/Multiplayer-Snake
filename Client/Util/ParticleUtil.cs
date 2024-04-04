using System;
using System.Collections.Generic;
using Client.Components;
using Microsoft.Xna.Framework;
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

    public static List<Entity> eatFood(Texture2D foodSheet, Entity food)
    {
        var rng = new ExtendedRandom();
        var particles = new List<Entity>();
        var pos = food.get<Position>();
        var appearance = food.get<Appearance>();

        var size = appearance.frameWidth / 3;
        
        var t = new Texture2D(Client.mGraphics.GraphicsDevice, size, size);
        var data = new Color[size * size];
        var rec = new Rectangle((appearance.staticFrame ?? 0) * appearance.frameWidth + size, size, size, size);
        foodSheet.GetData(0, rec, data, 0, size * size);
        t.SetData(data);
        
        for (int i = 0; i < 10; i++)
        {
            particles.Add(foodParticle(t, pos.x, pos.y, rng));
        }

        return particles;
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

    public static Entity foodParticle(Texture2D texture, float x, float y, ExtendedRandom rng)
    {
        // return Entities.Particle.create(texture,
        //     x, y,
        //     rng,
        //     5, 2,
        //     (float)rng.nextCircleAngle(), 0f,
        //     250f, 50f,
        //     0.1f, 0.01f,
        //     (float)-Math.PI / 2,
        //     10f, 2f);
        return Entities.Particle.create(texture,
            x, y,
            rng,
            3, 1,
            (float)rng.nextCircleAngle(), 0f,
            200f, 50f,
            0.125f, 0.01f,
            (float)-Math.PI / 2,
            6f, 3f);
    }
}