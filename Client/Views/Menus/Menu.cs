using System.Collections.Generic;
using Client.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Client.Views.Menus;

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
        mMouseInput.clearRegions();
        const int dasDelay = 500;
        const int dasPeriod = 75;
        var t = new DASTimer(dasDelay, dasPeriod);
        mKeyboardInput.registerCommand(InputDevice.Commands.UP, _ => moveUp(), gt => t.tick(gt, moveUp), t.resetTimer);
        mKeyboardInput.registerCommand(InputDevice.Commands.DOWN, _ => moveDown(), gt => t.tick(gt, moveDown), t.resetTimer);
        mKeyboardInput.registerCommand(InputDevice.Commands.LEFT, _ => moveLeft(), gt => t.tick(gt, moveLeft), t.resetTimer);
        mKeyboardInput.registerCommand(InputDevice.Commands.RIGHT, _ => moveRight(), gt => t.tick(gt, moveRight), t.resetTimer);
        mKeyboardInput.registerCommand(InputDevice.Commands.SELECT, _ => mSelected?.OnSelect());
        
        mMouseInput.registerMouseRegion(null, MouseInput.MouseActions.L_CLICK, null, null, _ => mSelected?.OnSelect());
    }

    public override void loadContent(ContentManager contentManager)
    {
        mFont = contentManager.Load<SpriteFont>("Fonts/menu");
        mFontSelect = contentManager.Load<SpriteFont>("Fonts/menu-select");
        mButtonBackground = contentManager.Load<Texture2D>("Images/square");
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

    protected void registerHoverRegion(MenuOption option)
    {
        mMouseInput.registerMouseRegion(option.getRectangle(), MouseInput.MouseActions.HOVER, _ => mSelected = option, _ => mSelected = option, _ => mSelected = null);
    }

    public override void update(GameTime gameTime)
    {
        mKeyboardInput.update(gameTime.ElapsedGameTime);
        if (mGame.IsActive) mMouseInput.update(gameTime.ElapsedGameTime);
    }

    public override void render(GameTime gameTime)
    {
        mSpriteBatch.Begin();
        foreach (var option in mOptions)
        {
            var font = mFont;
            var fontColor = Color.White;
            if (mSelected == option)
            {
                font = mFontSelect;
                fontColor = Color.Red;
            }
            option.render(gameTime, mSpriteBatch, font, mButtonBackground, fontColor, Color.Black);
        }
        mSpriteBatch.End();
    }
}