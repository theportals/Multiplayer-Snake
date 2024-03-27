using System;
using System.Collections.Generic;
using Client.Util;
using Microsoft.Xna.Framework;

namespace Client.Particles;

public class ParticleType
{
    private Dictionary<long, Particle> m_particles = new();
    public Dictionary<long, Particle>.ValueCollection particles => m_particles.Values;
    private ExtendedRandom m_random = new();

    private int m_sizeMean; // pixels
    private int m_sizeStdDev;   // pixels
    private float m_speedMean;  // pixels per millisecond
    private float m_speedStDev; // pixles per millisecond
    private float m_lifetimeMean; // milliseconds
    private float m_lifetimeStdDev; // milliseconds
    private float m_rotation;
    private float m_rotationStdDev;

    public ParticleType(int sizeMean, int sizeStdDev, float speedMean, float speedStdDev, int lifetimeMean, int lifetimeStdDev, float rotation, float mRotationStdDev)
    {
        m_sizeMean = sizeMean;
        m_sizeStdDev = sizeStdDev;
        m_speedMean = speedMean;
        m_speedStDev = speedStdDev;
        m_lifetimeMean = lifetimeMean;
        m_lifetimeStdDev = lifetimeStdDev;
        m_rotation = rotation;
        m_rotationStdDev = mRotationStdDev;
    }

    private Particle create(Vector2 loc, Vector2 dir)
    {
        var size = (float)m_random.nextGaussian(m_sizeMean, m_sizeStdDev);
        var speed = (float)m_random.nextGaussian(m_speedMean, m_speedStDev);
        if (speed < 0) speed *= -1;
        var p = new Particle(
                loc,
                dir,
                speed,
                new Vector2(size, size),
                new TimeSpan(0, 0, 0, 0, (int)(m_random.nextGaussian(m_lifetimeMean, m_lifetimeStdDev))),
                (float)m_random.nextGaussian(m_rotation, m_rotationStdDev));

        return p;
    }

    public void update(GameTime gameTime)
    {
        // Update existing particles
        List<long> removeMe = new List<long>();
        foreach (Particle p in m_particles.Values)
        {
            if (!p.update(gameTime))
            {
                removeMe.Add(p.name);
            }
        }

        // Remove dead particles
        foreach (long key in removeMe)
        {
            m_particles.Remove(key);
        }
    }

    public void spawn(Vector2 loc, float angle, float angleStDev, int amount)
    {
        // Generate some new particles
        for (int i = 0; i < amount; i++)
        {
            var particle = create(loc, m_random.nextGaussianVector(angle, angleStDev));
            m_particles.Add(particle.name, particle);
        }
    }

    public void spawn(Vector2 loc, int amount)
    {
        // Generate some new particles
        for (int i = 0; i < amount; i++)
        {
            var particle = create(loc, m_random.nextCircleVector());
            m_particles.Add(particle.name, particle);
        }
    }

    public void RemoveAll()
    {
        foreach (var p in m_particles.Keys)
        {
            m_particles.Remove(p);
        }
    }
}