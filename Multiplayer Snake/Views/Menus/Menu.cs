using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Multiplayer_Snake.Input;
using Multiplayer_Snake.Util;

namespace Multiplayer_Snake.Views.Menus;

public abstract class Menu : GameStateView
{
    protected SpriteFont mFont;
    protected SpriteFont mFontSelect;
    protected Texture2D mButtonBackground;
    protected List<MenuOption> mOptions;
    protected MenuOption? mSelected;
    protected MenuOption mDefault;

    public override void initializeSession()
    {
        mKeyboardInput.clearCommands();
        const int dasDelay = 500;
        const int dasPeriod = 75;
        var t = new DASTimer(dasDelay, dasPeriod);
        mKeyboardInput.registerCommand(KeyboardInput.Commands.UP, _ => moveUp(), gt => t.tick(gt, moveUp), t.resetTimer);
        mKeyboardInput.registerCommand(KeyboardInput.Commands.DOWN, _ => moveDown(), gt => t.tick(gt, moveDown), t.resetTimer);
        mKeyboardInput.registerCommand(KeyboardInput.Commands.LEFT, _ => moveLeft(), gt => t.tick(gt, moveLeft), t.resetTimer);
        mKeyboardInput.registerCommand(KeyboardInput.Commands.RIGHT, _ => moveRight(), gt => t.tick(gt, moveRight), t.resetTimer);
        mKeyboardInput.registerCommand(KeyboardInput.Commands.SELECT, _ => mSelected?.OnSelect());
    }

    private void moveUp()
    {
        if (mSelected == null) mSelected = mDefault;
        else if (mSelected.up != null) mSelected = mSelected.up;
    }

    private void moveDown()
    {
        if (mSelected == null) mSelected = mDefault;
        else if (mSelected.down != null) mSelected = mSelected.down;
    }

    private void moveLeft()
    {
        if (mSelected == null) mSelected = mDefault;
        else if (mSelected.left != null) mSelected = mSelected.left;
    }

    private void moveRight()
    {
        if (mSelected == null) mSelected = mDefault;
        else if (mSelected.right != null) mSelected = mSelected.right;
    }

    public override void update(GameTime gameTime)
    {
        mKeyboardInput.update(gameTime);
    }

    public override void render(GameTime gameTime)
    {
        mSpriteBatch.Begin();
        foreach (var option in mOptions)
        {
            var font = mFont;
            if (mSelected == option) font = mFontSelect;
            option.render(gameTime, mSpriteBatch, font, mButtonBackground);
        }
        mSpriteBatch.End();
    }
}