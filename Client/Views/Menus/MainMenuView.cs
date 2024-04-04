using System;
using System.Collections.Generic;
using Client.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Views.Menus;

public class MainMenuView : Menu
{
    private TextInput mNameInput;
    private string nameError = "";
    private Texture2D logo;

    public override void loadContent(ContentManager contentManager)
    {
        base.loadContent(contentManager);
        logo = contentManager.Load<Texture2D>("Images/logo");
        
    }

    public override void initializeSession()
    {
        base.initializeSession();
        var textSize = mFont.MeasureString("____________");
        mNameInput = new TextInput(mKeyboardInput, mMouseInput, 
            mGraphics.PreferredBackBufferWidth / 2, 9 * mGraphics.PreferredBackBufferHeight / 16, 
            (int)textSize.X, (int)textSize.Y, 
            mButtonBackground, mSpriteBatch, mFont,
            n =>
            {
                mMenuSelectSound.Play();
                this.submitName(n);
            },
            mGame.playerName);
        mSelected = null;
        var start = 2 * mGraphics.PreferredBackBufferHeight / 3;
        const int spacing = 100;
        var newGame = new MenuOption("New Game", () =>
        {
            if (mGame.playerName == "")
            {
                nameError = "Please choose a name!";
                return;
            }
            if (mGame.tutorialCompleted) mGame.changeState(GameStates.GAMEPLAY);
            else mGame.changeState(GameStates.TUTORIAL);
        }, mGraphics.PreferredBackBufferWidth / 3, start, mFont);
        var highScores = new MenuOption("High Scores", () => mGame.changeState(GameStates.HIGH_SCORES), mGraphics.PreferredBackBufferWidth / 3, start + spacing, mFont);
        var controls = new MenuOption("Controls", () => mGame.changeState(GameStates.CONTROLS), 2 * mGraphics.PreferredBackBufferWidth / 3, start, mFont);
        var credits = new MenuOption("Credits", () => mGame.changeState(GameStates.CREDITS), 2 * mGraphics.PreferredBackBufferWidth / 3, start + spacing, mFont);
        var exit = new MenuOption("Exit", () => mGame.changeState(GameStates.EXIT), mGraphics.PreferredBackBufferWidth / 2, start + spacing * 2, mFont);
        var submitName = new MenuOption("Submit", () => this.submitName(mNameInput.input),
            (int)(mGraphics.PreferredBackBufferWidth / 2 + textSize.X), 9 * mGraphics.PreferredBackBufferHeight / 16, mFont);

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
        registerHoverRegion(submitName);
        
        mMouseInput.registerMouseRegion(mNameInput.getRectangle(), MouseInput.MouseActions.L_CLICK, _ => mNameInput.focus());
        mMouseInput.registerMouseRegion(mNameInput.getRectangle(), MouseInput.MouseActions.L_CLICK, _ => mNameInput.unFocus(), null, null, true);
        
        
        mOptions = new List<MenuOption>
        {
            newGame,
            highScores,
            controls,
            credits,
            exit,
            submitName
        };
        mButtonBackdrop = 
            new Rectangle(3 * mGraphics.PreferredBackBufferWidth / 16, 14 * mGraphics.PreferredBackBufferHeight / 32, 10 * mGraphics.PreferredBackBufferWidth / 16, mGraphics.PreferredBackBufferHeight / 2);
        mDefault = newGame;
        if (!mGame.tutorialCompleted) return;
        
        var tutorial = new MenuOption("Tutorial", () =>
            {
                if (mGame.playerName == "")
                {
                    nameError = "Please choose a name!";
                    return;
                }
                mGame.changeState(GameStates.TUTORIAL);
            },
            mGraphics.PreferredBackBufferWidth / 2, start, mFont);
        registerHoverRegion(tutorial);
        mOptions.Add(tutorial);
        newGame.linkRight(tutorial);
        tutorial.linkRight(controls);
    }

    private void submitName(string name)
    {
        if (name.Length < 1)
        {
            nameError = "Please choose a name!";
            return;
        }

        nameError = "";
        mGame.playerName = name;
    }

    public override void update(GameTime gameTime)
    {
        mKeyboardInput.update(gameTime.ElapsedGameTime, true);
        if (mGame.IsActive) mMouseInput.update(gameTime.ElapsedGameTime);
        mNameInput.update(gameTime);
        mKeyboardInput.endUpdate();
    }

    public override void render(GameTime gameTime)
    {
        base.render(gameTime);
        mNameInput.render(gameTime);
        mSpriteBatch.Begin();
        mSpriteBatch.Draw(logo, new Vector2((mGraphics.PreferredBackBufferWidth - logo.Width) / 2, mGraphics.PreferredBackBufferHeight / 4 - logo.Height / 2), Color.White);
        if (nameError != "")
        {
            var size = mFont.MeasureString(nameError);
            mSpriteBatch.DrawString(mFont, nameError, new Vector2((mGraphics.PreferredBackBufferWidth - size.X) / 2, 15 * mGraphics.PreferredBackBufferHeight / 32), Color.Red);
        }
        mSpriteBatch.End();
    }
}