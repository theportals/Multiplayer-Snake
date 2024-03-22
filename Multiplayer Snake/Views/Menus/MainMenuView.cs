using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Multiplayer_Snake.Input;

namespace Multiplayer_Snake.Views.Menus;

public class MainMenuView : Menu
{
    public override void initializeSession()
    {
        base.initializeSession();
        var test = new MenuOption("New Game", () => mGame.changeState(GameStates.GAMEPLAY), mGraphics.PreferredBackBufferWidth / 4, 150, 150, 50);
        var test2 = new MenuOption("Test2", () => Console.WriteLine("Test2"), mGraphics.PreferredBackBufferWidth / 4, 225, 150, 50);
        var test3 = new MenuOption("Test3", () => Console.WriteLine("Test3"), 3 * mGraphics.PreferredBackBufferWidth / 4, 150, 150, 50);
        var test4 = new MenuOption("Test4", () => Console.WriteLine("Test4"), 3 * mGraphics.PreferredBackBufferWidth / 4, 225, 150, 50);
        var test5 = new MenuOption("Test5", () => Console.WriteLine("Test5"), mGraphics.PreferredBackBufferWidth / 2, 300, 150, 50);

        test.linkDown(test2);
        test.linkRight(test3);
        
        test2.linkRight(test4);
        test2.linkDown(test5, false);
        
        test3.linkDown(test4);
        
        test4.linkDown(test5, false);
        
        test5.linkLeft(test2, false);
        test5.linkRight(test4, false);
        
        mMouseInput.registerMouseRegion(test.getRectangle(), MouseInput.MouseActions.HOVER, _ => mSelected = test, null, _ => mSelected = null);
        mMouseInput.registerMouseRegion(test2.getRectangle(), MouseInput.MouseActions.HOVER, _ => mSelected = test2, null, _ => mSelected = null);
        mMouseInput.registerMouseRegion(test3.getRectangle(), MouseInput.MouseActions.HOVER, _ => mSelected = test3, null, _ => mSelected = null);
        mMouseInput.registerMouseRegion(test4.getRectangle(), MouseInput.MouseActions.HOVER, _ => mSelected = test4, null, _ => mSelected = null);
        mMouseInput.registerMouseRegion(test5.getRectangle(), MouseInput.MouseActions.HOVER, _ => mSelected = test5, null, _ => mSelected = null);
        mMouseInput.registerMouseRegion(null, MouseInput.MouseActions.L_CLICK, null, null, _ => mSelected?.OnSelect());
        
        
        mOptions = new List<MenuOption>
        {
            test,
            test2,
            test3,
            test4,
            test5
        };
        mDefault = test;
    }

    public override void loadContent(ContentManager contentManager)
    {
        mFont = contentManager.Load<SpriteFont>("Fonts/menu");
        mFontSelect = contentManager.Load<SpriteFont>("Fonts/menu-select");
        mButtonBackground = contentManager.Load<Texture2D>("Images/square");
    }
}