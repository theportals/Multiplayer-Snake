using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Views.Menus;

public class MenuOption
{
    private string mTitle;
    public Action OnSelect { get; }
    private int mX;
    private int mY;
    private SpriteFont mBasic;
    private SpriteFont mSelected;
    private Rectangle mRec;
    
    public MenuOption up { get; private set; }
    public MenuOption down { get; private set; }
    public MenuOption left { get; private set; }
    public MenuOption right { get; private set; }

    public MenuOption(string title, Action onSelect, int x, int y, SpriteFont basic, MenuOption up=null, MenuOption down=null, MenuOption left=null, MenuOption right=null)
    {
        mTitle = title;
        OnSelect = onSelect;
        mX = x;
        mY = y;
        mBasic = basic;
        this.up = up;
        this.down = down;
        this.left = left;
        this.right = right;
        var size = mBasic.MeasureString(mTitle);
        const int padding = 25;
        mRec = new Rectangle((int)(mX - (size.X + padding) / 2), (int)(mY - size.Y / 2), (int)(size.X + padding), (int)size.Y);
    }

    public Rectangle getRectangle()
    {
        // return new Rectangle(mX - mWidth/2, mY - mHeight/2, mWidth, mHeight);
        return mRec;
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
        var size = font.MeasureString(mTitle);
        var w = size.X + 25;
        spriteBatch.Draw(background, new Rectangle((int)(mX - w/2), (int)(mY - size.Y/2), (int)(w), (int)size.Y) , buttonColor);
        spriteBatch.DrawString(font, mTitle, new Vector2(mX - size.X / 2, mY - size.Y / 2), fontColor);
    }
}