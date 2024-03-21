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
    private float mWidth;
    private float mHeight;
    
    public MenuOption up { get; }
    public MenuOption down { get; }
    public MenuOption left { get; }
    public MenuOption right { get; }

    public MenuOption(string title, Action onSelect, int x, int y, float width, float height, MenuOption up=null, MenuOption down=null, MenuOption left=null, MenuOption right=null)
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

    public void render(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont font, Texture2D background)
    {
        
    }
}