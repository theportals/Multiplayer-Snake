using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Particles;

public class ParticleManager
{
    public enum ParticleTypes
    {
        FIRE,
        SMOKE
    }

    private Dictionary<ParticleTypes, ParticleType> m_particleDict = new();
    private Dictionary<ParticleTypes, bool> m_particleLoadedDict = new();
    private Dictionary<ParticleTypes, ParticleRenderer> m_particleRendererDict = new();

    public void LoadAll(ContentManager content)
    {
        foreach (var type in Enum.GetValues<ParticleTypes>())
        {
            LoadParticle(type, content);
        }
    }

    public void LoadParticle(ParticleTypes type, ContentManager content)
    {
        switch (type)
        {
            case ParticleTypes.FIRE:
                LoadAndAdd(ParticleTypes.FIRE, 
                    new ParticleType(
                        10, 4,
                        0.12f, 0.05f,
                        500, 50,
                        0.5f, 0.5f),
                    "fire",content);
                break;
            case ParticleTypes.SMOKE:
                LoadAndAdd(ParticleTypes.SMOKE, new ParticleType(
                    15, 4,
                    0.12f, 0.05f,
                    3000, 1000,
                    0.5f, 0.5f), 
                    "smoke-2", content);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private void LoadAndAdd(ParticleTypes type, ParticleType p, string name, ContentManager content)
    {
        m_particleDict.Add(type, p);
        m_particleLoadedDict.Add(type, true);
        var render = new ParticleRenderer(name);
        render.LoadContent(content);
        m_particleRendererDict.Add(type, render);
    }

    public bool IsLoaded(ParticleTypes type)
    {
        return m_particleLoadedDict.ContainsKey(type) && m_particleLoadedDict[type];
    }

    public void Spawn(ParticleTypes type, Vector2 pos, float angle, float angleStdDev, int amount)
    {
        if (!IsLoaded(type)) return;
        m_particleDict[type].spawn(pos, angle, angleStdDev, amount);
    }

    public void Spawn(ParticleTypes type, Vector2 pos, int amount)
    {
        if (!IsLoaded(type)) return;
        m_particleDict[type].spawn(pos, amount);
    }

    public void Update(GameTime gameTime)
    {
        foreach (var p in Enum.GetValues<ParticleTypes>().Where(IsLoaded))
        {
            m_particleDict[p].update(gameTime);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // spriteBatch.End();
        // foreach (var p in Enum.GetValues<ParticleTypes>().Where(IsLoaded))
        // {
        //     m_particleRendererDict[p].draw(spriteBatch, m_particleDict[p]);
        // }
        // spriteBatch.Begin();
    }

    public void ClearParticles()
    {
        foreach (var p in Enum.GetValues<ParticleTypes>().Where(IsLoaded))
        {
            m_particleDict[p].RemoveAll();
        }
    }
}