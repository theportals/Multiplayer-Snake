using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Util;

public class DrawUtil
{
    public static void DrawStringsCentered(List<String> strings, SpriteFont font, Color c, SpriteBatch spriteBatch)
    {
        for (int i = 0; i < strings.Count; i++)
        {
            var s = strings[i];
            var l = font.MeasureString(s);
            spriteBatch.DrawString(font, s, new Vector2(spriteBatch.GraphicsDevice.Viewport.Width / 2f - l.X / 2, spriteBatch.GraphicsDevice.Viewport.Height/2f - l.Y / 2 + font.LineSpacing * i), c);
        }
    }
    public static void DrawGrayOverlay(SpriteBatch spriteBatch, float alpha=0.5f)
    {
        spriteBatch.Draw(Client.pixel,
            new Rectangle(0, 0, spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height),
            new Color(Color.Black, alpha));
    }
}