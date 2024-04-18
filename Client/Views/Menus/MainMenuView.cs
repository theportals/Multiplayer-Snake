using System.Collections.Generic;
using Client.Input;
using Client.Util;
using Client.Views;
using Client.Views.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Multiplayer_Snake.Views.Menus;

public class MainMenuView : Menu
{
    private Texture2D logo;

    public override void loadContent(ContentManager contentManager)
    {
        base.loadContent(contentManager);
        logo = contentManager.Load<Texture2D>("Images/logo");
        
    }

    public override void initializeSession()
    {
        base.initializeSession();
        mSelected = null;
        var start = 2 * mGraphics.PreferredBackBufferHeight / 3;
        const int spacing = 100;
        var newGame = new MenuOption("New Game", () =>
        {
            if (Client.Client.playerName == "") mGame.changeState(GameStates.NAME_SELECT);
            else if (!mGame.tutorialCompleted) mGame.changeState(GameStates.TUTORIAL);
            else mGame.changeState(GameStates.GAMEPLAY);
        }, mGraphics.PreferredBackBufferWidth / 3, start, mFont);
        var highScores = new MenuOption("High Scores", () => mGame.changeState(GameStates.HIGH_SCORES), mGraphics.PreferredBackBufferWidth / 3, start + spacing, mFont);
        var controls = new MenuOption("Controls", () => mGame.changeState(GameStates.CONTROLS), 2 * mGraphics.PreferredBackBufferWidth / 3, start, mFont);
        var credits = new MenuOption("Credits", () => mGame.changeState(GameStates.CREDITS), 2 * mGraphics.PreferredBackBufferWidth / 3, start + spacing, mFont);
        var exit = new MenuOption("Exit", () => mGame.changeState(GameStates.EXIT), mGraphics.PreferredBackBufferWidth / 2, start + spacing * 2, mFont);

        newGame.linkDown(highScores);
        newGame.linkRight(controls);
        
        highScores.linkRight(credits);
        highScores.linkDown(exit, false);
        
        controls.linkDown(credits);
        
        credits.linkDown(exit, false);
        
        exit.linkLeft(highScores, false);
        exit.linkRight(credits, false);
        
        registerHoverRegion(newGame);
        registerHoverRegion(highScores);
        registerHoverRegion(controls);
        registerHoverRegion(credits);
        registerHoverRegion(exit);
        
        mOptions = new List<MenuOption>
        {
            newGame,
            highScores,
            controls,
            credits,
            exit
        };
        mButtonBackdrop = 
            new Rectangle(3 * mGraphics.PreferredBackBufferWidth / 16, 14 * mGraphics.PreferredBackBufferHeight / 32, 10 * mGraphics.PreferredBackBufferWidth / 16, mGraphics.PreferredBackBufferHeight / 2);
        mDefault = newGame;

        MenuOption changeName = null;
        if (Client.Client.playerName != "")
        {
            changeName = new MenuOption("Change Name", () => { mGame.changeState(GameStates.NAME_SELECT); },
                mGraphics.PreferredBackBufferWidth / 2, start, mFont);
            registerHoverRegion(changeName);
            mOptions.Add(changeName);
            newGame.linkRight(changeName);
            changeName.linkRight(controls);
        }

        MenuOption tutorial = null;
        if (mGame.tutorialCompleted)
        {

            tutorial = new MenuOption("Tutorial", () => { mGame.changeState(GameStates.TUTORIAL); },
                mGraphics.PreferredBackBufferWidth / 2, start + spacing, mFont);
            registerHoverRegion(tutorial);
            mOptions.Add(tutorial);
            highScores.linkRight(tutorial);
            tutorial.linkRight(credits);
            tutorial.linkDown(exit);
            changeName?.linkDown(tutorial);
        }
    }

    public override void render(GameTime gameTime)
    {
        base.render(gameTime);
        mSpriteBatch.Begin();
        mSpriteBatch.Draw(logo, new Vector2((mGraphics.PreferredBackBufferWidth - logo.Width) / 2, mGraphics.PreferredBackBufferHeight / 4 - logo.Height / 2), Color.White);
        if (Client.Client.playerName != "") DrawUtil.DrawStringsCentered(new List<string> { $"Your Name: {Client.Client.playerName}" }, mFont, Color.White, mSpriteBatch);
        mSpriteBatch.End();
    }
}