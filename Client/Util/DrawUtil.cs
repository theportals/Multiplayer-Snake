using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Util;

public class DrawUtil
{
    public static void DrawStringsCentered(List<String> strings, SpriteFont font, Color c, SpriteBatch spriteBatch, int? startX=null, int? startY=null)
    {
        int x;
        if (startX.HasValue) x = startX.Value;
        else x = spriteBatch.GraphicsDevice.Viewport.Width / 2;
        int y;
        if (startY.HasValue) y = startY.Value;
        else y = spriteBatch.GraphicsDevice.Viewport.Height / 2;
        
        for (int i = 0; i < strings.Count; i++)
        {
            var s = strings[i];
            var l = font.MeasureString(s);
            spriteBatch.DrawString(font, s, new Vector2(x - l.X / 2, y - l.Y / 2 + font.LineSpacing * i), c);
        }
    }
    public static void DrawGrayOverlay(SpriteBatch spriteBatch, float alpha=0.5f)
    {
        spriteBatch.Draw(Client.pixel,
            new Rectangle(0, 0, spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height),
            new Color(Color.Black, alpha));
    }
}