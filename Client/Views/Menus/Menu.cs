using System;
using System.Collections.Generic;
using Client.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Client.Views.Menus;

public abstract class Menu : GameStateView
{
    protected SpriteFont mFont;
    protected SpriteFont mFontSelect;
    protected Texture2D mButtonBackground;
    protected Texture2D bg;
    protected List<MenuOption> mOptions;
    protected MenuOption? mSelected;
    protected MenuOption mDefault;
    protected SoundEffect mMenuSelectSound;
    protected SoundEffect mMenuBrowseSound;

    protected Rectangle mButtonBackdrop;

    protected bool drawBackground = true;
    public override void initializeSession()
    {
        mKeyboardInput.clearCommands();
        mMouseInput.clearRegions();
        const int dasDelay = 500;
        const int dasPeriod = 75;
        var t = new DASTimer(dasDelay, dasPeriod);
        mKeyboardInput.registerCommand(InputDevice.Commands.UP, _ => moveUp(), gt => t.tick(gt, moveUp), t.resetTimer);
        mKeyboardInput.registerCommand(InputDevice.Commands.DOWN, _ => moveDown(), gt => t.tick(gt, moveDown), t.resetTimer);
        mKeyboardInput.registerCommand(InputDevice.Commands.LEFT, _ => moveLeft(), gt => t.tick(gt, moveLeft), t.resetTimer);
        mKeyboardInput.registerCommand(InputDevice.Commands.RIGHT, _ => moveRight(), gt => t.tick(gt, moveRight), t.resetTimer);
        mKeyboardInput.registerCommand(InputDevice.Commands.SELECT, _ =>
        {
            if (mSelected != null)
            {
                mSelected.OnSelect();
                mMenuSelectSound.Play();
            }
        });
        
        mMouseInput.registerMouseRegion(null, MouseInput.MouseActions.L_CLICK, null, null, _ =>
        {
            if (mSelected != null)
            {
                mSelected.OnSelect();
                mMenuSelectSound.Play();
            }
        });
    }

    public override void loadContent(ContentManager contentManager)
    {
        mFont = contentManager.Load<SpriteFont>("Fonts/menu");
        mFontSelect = contentManager.Load<SpriteFont>("Fonts/menu-select");
        mButtonBackground = contentManager.Load<Texture2D>("Images/square");
        mMenuSelectSound = contentManager.Load<SoundEffect>("Sounds/button_select");
        mMenuBrowseSound = contentManager.Load<SoundEffect>("Sounds/button_click");
        bg = contentManager.Load<Texture2D>("Images/normal_hillside");
    }

    protected void moveUp()
    {
        if (mSelected == null) mSelected = mDefault;
        else if (mSelected.up != null) {
            mMenuBrowseSound.Play();
            mSelected = mSelected.up;
        }
    }

    protected void moveDown()
    {
        if (mSelected == null) mSelected = mDefault;
        else if (mSelected.down != null) {
            mMenuBrowseSound.Play();
            mSelected = mSelected.down;
        }
    }

    protected void moveLeft()
    {
        if (mSelected == null) mSelected = mDefault;
        else if (mSelected.left != null) {
            mMenuBrowseSound.Play();
            mSelected = mSelected.left;
        }
    }

    protected void moveRight()
    {
        if (mSelected == null) mSelected = mDefault;
        else if (mSelected.right != null) {
            mMenuBrowseSound.Play();
            mSelected = mSelected.right;
        }
    }

    protected void registerHoverRegion(MenuOption option)
    {
        mMouseInput.registerMouseRegion(option.getRectangle(), MouseInput.MouseActions.HOVER, _ =>
        {
            mSelected = option;
            mMenuBrowseSound.Play();
        }, _ => mSelected = option, _ => mSelected = null);
    }

    public override void update(GameTime gameTime)
    {
        mKeyboardInput.update(gameTime.ElapsedGameTime);
        if (mGame.IsActive) mMouseInput.update(gameTime.ElapsedGameTime);
    }

    public override void render(GameTime gameTime)
    {
        mSpriteBatch.Begin();
        if (drawBackground) mSpriteBatch.Draw(bg, new Rectangle(0, 0, mGraphics.PreferredBackBufferWidth, mGraphics.PreferredBackBufferHeight), new Color(25, 25, 255));
        mSpriteBatch.Draw(mButtonBackground, mButtonBackdrop,new Color(64, 64, 64));
        foreach (var option in mOptions)
        {
            var font = mFont;
            var fontColor = Color.Black;
            if (mSelected == option)
            {
                font = mFontSelect;
                fontColor = Color.Red;
            }
            option.render(gameTime, mSpriteBatch, font, mButtonBackground, fontColor, Color.White);
        }
        mSpriteBatch.End();
    }
}