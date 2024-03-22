using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Multiplayer_Snake.Views.Menus;

public class MenuOption
{
    private string mTitle;
    public Action OnSelect { get; }
    private int mX;
    private int mY;
    private int mWidth;
    private int mHeight;
    
    public MenuOption up { get; private set; }
    public MenuOption down { get; private set; }
    public MenuOption left { get; private set; }
    public MenuOption right { get; private set; }

    public MenuOption(string title, Action onSelect, int x, int y, int width, int height, MenuOption up=null, MenuOption down=null, MenuOption left=null, MenuOption right=null)
    {
        mTitle = title;
        OnSelect = onSelect;
        mX = x;
        mY = y;
        mWidth = width;
        mHeight = height;
        this.up = up;
        this.down = down;
        this.left = left;
        this.right = right;
    }

    public Rectangle getRectangle()
    {
        return new Rectangle(mX - mWidth/2, mY - mHeight/2, mWidth, mHeight);
    }

    public void linkUp(MenuOption newUp, bool symmetric = true)
    {
        up = newUp;
        if (symmetric) newUp.down = this;
    }

    public void linkDown(MenuOption newDown, bool symmetric = true)
    {
        down = newDown;
        if (symmetric) newDown.up = this;
    }

    public void linkLeft(MenuOption newLeft, bool symmetric = true)
    {
        left = newLeft;
        if (symmetric) newLeft.right = this;
    }

    public void linkRight(MenuOption newRight, bool symmetric = true)
    {
        right = newRight;
        if (symmetric) newRight.left = this;
    }

    public void render(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont font, Texture2D background, Color fontColor, Color buttonColor)
    {
        spriteBatch.Draw(background, new Rectangle((int)(mX - mWidth/2), (int)(mY - mHeight/2), mWidth, mHeight) , buttonColor);
        var size = font.MeasureString(mTitle);
        spriteBatch.DrawString(font, mTitle, new Vector2(mX - size.X / 2, mY - size.Y / 2), fontColor);
    }
}