using System;
using Client.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Client.Entities;
using Shared.Components;
using Shared.Entities;

namespace Client.Systems;

public class Renderer : Shared.Systems.System
{
    public readonly int ARENA_SIZE;
    public readonly int WINDOW_WIDTH;
    public readonly int WINDOW_HEIGHT;
    public int OFFSET_X;
    public int OFFSET_Y;
    private readonly SpriteBatch mSpriteBatch;
    private readonly Texture2D mBackground;
    private readonly SpriteFont mFont;
    public float zoom { get; set; }
    private Entity? mFollow;

    public Renderer(SpriteBatch spriteBatch, SpriteFont font, Texture2D background, int windowWidth, int windowHeight, int arenaSize, Entity? toFollow, float zoom=1)
        : base(typeof(Appearance), typeof(Shared.Components.Position), typeof(Components.Sprite))
    {
        mFont = font;
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

    public bool isFollowing()
    {
        return mFollow != null;
    }
    
    public override void update(TimeSpan gameTime)
    {
        mSpriteBatch.Begin();
        mSpriteBatch.Draw(mBackground, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), new Color(25, 25, 255));

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
            OFFSET_X = (WINDOW_WIDTH - ARENA_SIZE) / 2;
            OFFSET_Y = (WINDOW_HEIGHT - ARENA_SIZE) / 2;
            var followPos = mFollow.get<Shared.Components.Position>();
            centerPoint.X = followPos.x;
            centerPoint.Y = followPos.y;
            bgCenter.X = OFFSET_X - centerPoint.X * zoom + ARENA_SIZE / 2;
            bgCenter.Y = OFFSET_Y - centerPoint.Y * zoom + ARENA_SIZE / 2;
        }
        Rectangle background = new Rectangle((int)(bgCenter.X), (int)(bgCenter.Y), (int)(ARENA_SIZE * zoom), (int)(ARENA_SIZE * zoom));
        mSpriteBatch.Draw(Client.pixel, background, Color.DarkBlue);

        foreach (var entity in mEntities.Values)
        {
            renderEntity(entity, centerPoint);
        }
        mSpriteBatch.End();
    }

    private void renderEntity(Entity entity, Vector2 centerPoint)
    {
        var appearance = entity.get<Appearance>();
        var pos = entity.get<Shared.Components.Position>();
        var sprite = entity.get<Sprite>();
        Vector2 drawPos = new Vector2();

        for (int segment = 0; segment < pos.segments.Count; segment++)
        {
            drawPos = getDrawPos(centerPoint, pos, segment);

            float rot = 0f;
            if (segment > 0)
            {
                var leader = pos.segments[segment - 1];
                var follower = pos.segments[segment];
                rot = (float)Math.Atan2(leader.Y - follower.Y, leader.X - follower.X);
            }
            else if (entity.contains<Shared.Components.Movable>()) rot = entity.get<Shared.Components.Movable>().facing;

            if (entity.contains<RotationOffset>()) rot += entity.get<RotationOffset>().offset;
            
            var c = Color.White;
            if (entity.contains<ColorOverride>())
            {
                var color = entity.get<ColorOverride>().color;
                c.R = color.R;
                c.G = color.G;
                c.B = color.B;
                c.A = color.A;
            }
            if (entity.contains<Boostable>())
            {
                var boost = entity.get<Boostable>();
                var frac = boost.stamina / boost.maxStamina;
                var staminaColor = new Color(
                    (int)MathHelper.Lerp(255, c.R, frac),
                    (int)MathHelper.Lerp(0, c.G, frac),
                    (int)MathHelper.Lerp(0, c.B, frac)
                );
                c = staminaColor;
            }
            if (entity.contains<Collision>())
            {
                if (entity.get<Collision>().intangibility > 0) c.A = 112;
            }

            if (appearance.animated)
            {
                var frame = 0;
                if (entity.contains<Snakeitude>())
                {
                    if (segment == 0) frame = 0;
                    else if (segment == pos.segments.Count - 1) frame = 2;
                    else frame = 1;
                }

                if (appearance.staticFrame.HasValue)
                {
                    frame = appearance.staticFrame.Value;
                }
                mSpriteBatch.Draw(sprite.texture,
                    new Rectangle((int)drawPos.X, (int)drawPos.Y, (int)Math.Ceiling(appearance.displaySize * zoom),
                        (int)Math.Ceiling(appearance.displaySize * zoom)),
                    new Rectangle(frame * appearance.frameWidth, 0, appearance.frameWidth, appearance.frameHeight),
                    c,
                    (float)(rot + Math.PI / 2),
                    new Vector2(appearance.frameWidth / 2f, appearance.frameHeight / 2f),
                    SpriteEffects.None,
                    0);
                
            } else
            {
                mSpriteBatch.Draw(sprite.texture,
                    new Rectangle((int)drawPos.X, (int)drawPos.Y, (int)Math.Ceiling(appearance.displaySize * zoom),
                        (int)Math.Ceiling(appearance.displaySize * zoom)),
                    null,
                    c,
                    (float)(rot + Math.PI / 2),
                    new Vector2(sprite.texture.Width / 2f, sprite.texture.Height / 2f),
                    SpriteEffects.None,
                    0);
            }
        }
                
        if (entity.contains<PlayerInfo>())
        {
            drawPos = getDrawPos(centerPoint, pos, 0);
            
            var name = entity.get<PlayerInfo>().playerName;
            var size = mFont.MeasureString(name);
            mSpriteBatch.DrawString(mFont, 
                name, 
                new Vector2((drawPos.X - size.X / 2 * zoom), (drawPos.Y - (size.Y / 2 + 15) * zoom)), 
                Color.White,
                0f,
                new Vector2(0, 0),
                new Vector2(zoom, zoom),
                SpriteEffects.None,
                0f);
        }
    }

    private Vector2 getDrawPos(Vector2 centerPoint, Shared.Components.Position pos, int segment)
    {
        var drawPos = new Vector2();
        if (mFollow != null){
            drawPos.X = WINDOW_WIDTH / 2 - (centerPoint.X - pos.segments[segment].X) * zoom;
            drawPos.Y = WINDOW_HEIGHT / 2 - (centerPoint.Y - pos.segments[segment].Y) * zoom;
        } 
        else
        {
            drawPos.X = WINDOW_WIDTH - (centerPoint.X - pos.segments[segment].X * zoom);
            drawPos.Y = WINDOW_HEIGHT - (centerPoint.Y - pos.segments[segment].Y * zoom);
        }

        return drawPos;
    }
}