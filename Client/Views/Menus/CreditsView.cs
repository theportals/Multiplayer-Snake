using System.Collections.Generic;
using Client.Input;
using Client.Util;
using Client.Views.Menus;
using Microsoft.Xna.Framework;

namespace Client.Views;

public class CreditsView : Menu
{
    public override void initializeSession()
    {
        base.initializeSession();
        mSelected = null;
        var back = new MenuOption("Return", () => mGame.changeState(GameStates.MAIN_MENU),
            mGraphics.PreferredBackBufferWidth / 2, 2 * mGraphics.PreferredBackBufferHeight / 3, mFont);
        mDefault = back;
        
        registerHoverRegion(back);
        
        mOptions = new List<MenuOption>
        {
            back,
        };
    }

    public override void render(GameTime gameTime)
    {
        base.render(gameTime);
        mSpriteBatch.Begin();
        DrawUtil.DrawStringsCentered(new List<string>{"Programming by Tom Longhurst"}, mFont, Color.White, mSpriteBatch);
        mSpriteBatch.End();
    }
}