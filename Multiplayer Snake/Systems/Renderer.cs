using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Multiplayer_Snake.Entities;

namespace Multiplayer_Snake.Systems;

public class Renderer : System
{
    private readonly int ARENA_SIZE;
    private readonly int WINDOW_WIDTH;
    private readonly int WINDOW_HEIGHT;
    private readonly int OFFSET_X;
    private readonly int OFFSET_Y;
    private readonly SpriteBatch mSpriteBatch;
    private readonly Texture2D mBackground;

    public Renderer(SpriteBatch spriteBatch, Texture2D background, int windowWidth, int windowHeight, int arenaSize)
        : base(typeof(Components.Appearance), typeof(Components.Position))
    {
        ARENA_SIZE = arenaSize;
        WINDOW_WIDTH = windowWidth;
        WINDOW_HEIGHT = windowHeight;
        OFFSET_X = (windowWidth - arenaSize) / 2;
        OFFSET_Y = (windowHeight - arenaSize) / 2;
        mSpriteBatch = spriteBatch;
        mBackground = background;
    }
    
    public override void Update(GameTime gameTime)
    {
        mSpriteBatch.Begin();

        Rectangle background = new Rectangle(OFFSET_X, OFFSET_Y, ARENA_SIZE, ARENA_SIZE);
        mSpriteBatch.Draw(mBackground, background, Color.Blue);

        foreach (var entity in mEntities.Values)
        {
            renderEntity(entity);
        }

        mSpriteBatch.End();
    }

    private void renderEntity(Entity entity)
    {
        var appearance = entity.GetComponent<Components.Appearance>();
        var pos = entity.GetComponent<Components.Position>();
        Vector2 drawPos = new Vector2();

        for (int segment = 0; segment < pos.segments.Count; segment++)
        {
            drawPos.X = OFFSET_X + pos.segments[segment].X;
            drawPos.Y = OFFSET_Y + pos.segments[segment].Y;

            float rot = 0f;
            if (segment > 0)
            {
                var leader = pos.segments[segment - 1];
                var follower = pos.segments[segment];
                rot = (float)Math.Atan2(leader.Y - follower.Y, leader.X - follower.X);
            }
            else if (entity.ContainsComponent<Components.Movable>()) rot = entity.GetComponent<Components.Movable>().facing;
            mSpriteBatch.Draw(appearance.image, 
                new Rectangle((int)drawPos.X, (int)drawPos.Y, appearance.size, appearance.size), 
                null,
                appearance.color,
                (float)(rot + Math.PI / 2),
                new Vector2(appearance.image.Width / 2f, appearance.image.Height / 2f),
                SpriteEffects.None,
                0);
        }
    }
}