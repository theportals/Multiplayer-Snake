using System.Collections.Generic;
using Client.Util;
using Client.Views;
using Client.Views.Menus;
using Microsoft.Xna.Framework;

namespace Multiplayer_Snake.Views.Menus;

public class TutorialView : Menu
{
    public override void initializeSession()
    {
        base.initializeSession();
        var back = new MenuOption("Return", () => mGame.changeState(GameStates.MAIN_MENU),
            mGraphics.PreferredBackBufferWidth / 3, 3 * mGraphics.PreferredBackBufferHeight / 4, mFont);

        var toGame = new MenuOption("Continue to game", () =>
            {
                mGame.tutorialCompleted = true;
                mGame.changeState(GameStates.GAMEPLAY);
            },
            2 * mGraphics.PreferredBackBufferWidth / 3, 3 * mGraphics.PreferredBackBufferHeight / 4, mFont);

        mDefault = back;
        mSelected = null;
        
        back.linkRight(toGame);
        
        registerHoverRegion(back);
        registerHoverRegion(toGame);

        mOptions = new List<MenuOption>()
        {
            back,
            toGame
        };
    }

    public override void render(GameTime gameTime)
    {
        base.render(gameTime);
        mSpriteBatch.Begin();
        var strings = new List<string>();
        if (mKeyboardInput.listenKeys)
        {
            strings.Add("Use the arrow keys to steer left and right");
            strings.Add("Press space to boost");
        }
        else
        {
            strings.Add($"Your snake will follow your mouse");
            strings.Add($"Click and hold to boost");
        }
        strings.Add("");
        strings.Add("You can change between mouse and keyboard in the controls menu");
        
        DrawUtil.DrawStringsCentered(strings, mFont, Color.White, mSpriteBatch, null, mGraphics.PreferredBackBufferHeight / 4);
        mSpriteBatch.End();
    }
}