using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Client.Entities;
using Shared.Entities;

namespace Client.Systems;

public class Renderer : Shared.Systems.System
{
    private readonly int ARENA_SIZE;
    private readonly int WINDOW_WIDTH;
    private readonly int WINDOW_HEIGHT;
    private int OFFSET_X;
    private int OFFSET_Y;
    private readonly SpriteBatch mSpriteBatch;
    private readonly Texture2D mBackground;
    public float zoom { get; set; }
    private Entity? mFollow;

    public Renderer(SpriteBatch spriteBatch, Texture2D background, int windowWidth, int windowHeight, int arenaSize, Entity? toFollow, float zoom=1)
        : base(typeof(Components.Appearance), typeof(Shared.Components.Position))
    {
        ARENA_SIZE = arenaSize;
        WINDOW_WIDTH = windowWidth;
        WINDOW_HEIGHT = windowHeight;
        OFFSET_X = (windowWidth - arenaSize) / 2;
        OFFSET_Y = (windowHeight - arenaSize) / 2;
        mSpriteBatch = spriteBatch;
        mBackground = background;
        mFollow = toFollow;
        this.zoom = zoom;
    }

    public void follow(Entity entity)
    {
        mFollow = entity;
    }
    
    public override void Update(TimeSpan gameTime)
    {
        mSpriteBatch.Begin();

        var centerPoint = new Vector2();
        var bgCenter = new Vector2();
        if (mFollow == null)
        {
            OFFSET_X = (int)((WINDOW_WIDTH - ARENA_SIZE * zoom) / 2);
            OFFSET_Y = (int)((WINDOW_HEIGHT - ARENA_SIZE * zoom) / 2);
            centerPoint.X = OFFSET_X + ARENA_SIZE * zoom;
            centerPoint.Y = OFFSET_Y + ARENA_SIZE * zoom;
            bgCenter.X = OFFSET_X;
            bgCenter.Y = OFFSET_Y;
        }
        else
        {
            var followPos = mFollow.get<Shared.Components.Position>();
            centerPoint.X = followPos.x;
            centerPoint.Y = followPos.y;
            bgCenter.X = OFFSET_X - centerPoint.X * zoom + ARENA_SIZE / 2;
            bgCenter.Y = OFFSET_Y - centerPoint.Y * zoom + ARENA_SIZE / 2;
        }
        Rectangle background = new Rectangle((int)(bgCenter.X), (int)(bgCenter.Y), (int)(ARENA_SIZE * zoom), (int)(ARENA_SIZE * zoom));
        mSpriteBatch.Draw(mBackground, background, Color.Blue);

        foreach (var entity in mEntities.Values)
        {
            renderEntity(entity, centerPoint);
        }
        mSpriteBatch.End();
    }

    private void renderEntity(Entity entity, Vector2 centerPoint)
    {
        var appearance = entity.get<Components.Appearance>();
        var pos = entity.get<Shared.Components.Position>();
        Vector2 drawPos = new Vector2();

        for (int segment = 0; segment < pos.segments.Count; segment++)
        {
            if (mFollow != null){
                drawPos.X = WINDOW_WIDTH / 2 - (centerPoint.X - pos.segments[segment].X) * zoom;
                drawPos.Y = WINDOW_HEIGHT / 2 - (centerPoint.Y - pos.segments[segment].Y) * zoom;
            } 
            else
            {
                drawPos.X = WINDOW_WIDTH - (centerPoint.X - pos.segments[segment].X * zoom);
                drawPos.Y = WINDOW_HEIGHT - (centerPoint.Y - pos.segments[segment].Y * zoom);
            }

            float rot = 0f;
            if (segment > 0)
            {
                var leader = pos.segments[segment - 1];
                var follower = pos.segments[segment];
                rot = (float)Math.Atan2(leader.Y - follower.Y, leader.X - follower.X);
            }
            else if (entity.contains<Shared.Components.Movable>()) rot = entity.get<Shared.Components.Movable>().facing;
            mSpriteBatch.Draw(appearance.image, 
                new Rectangle((int)drawPos.X, (int)drawPos.Y, (int)Math.Ceiling(appearance.size * zoom), (int)Math.Ceiling(appearance.size * zoom)), 
                null,
                appearance.color,
                (float)(rot + Math.PI / 2),
                new Vector2(appearance.image.Width / 2f, appearance.image.Height / 2f),
                SpriteEffects.None,
                0);
        }
    }
}