using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Particles;

public class ParticleRenderer
{
    private string m_nameParticleContent;
    private Texture2D m_texParticle;

    public ParticleRenderer(string nameParticleContent)
    {
        m_nameParticleContent = nameParticleContent;
    }

    public void LoadContent(ContentManager content)
    {
        m_texParticle = content.Load<Texture2D>(m_nameParticleContent);
    }

    public void draw(SpriteBatch spriteBatch, ParticleType type)
    {
        // spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        //
        // Rectangle r = new Rectangle(0, 0, 0, 0);
        // Vector2 centerTexture = new Vector2(m_texParticle.Width / 2, m_texParticle.Height / 2);
        // foreach (Particle particle in type.particles)
        // {
        //     r.X = (int)particle.loc.X;
        //     r.Y = (int)particle.loc.Y;
        //     r.Width = (int)particle.size.X;
        //     r.Height = (int)particle.size.Y;
        //
        //     spriteBatch.Draw(
        //         m_texParticle,
        //         r,
        //         null,
        //         Color.White,
        //         particle.rotation,
        //         centerTexture,
        //         SpriteEffects.None,
        //         0);
        // }
        //
        // spriteBatch.End();
    }
}