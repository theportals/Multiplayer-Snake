using System.Collections.Generic;
using Client.Input;
using Client.Views;
using Client.Views.Menus;
using Microsoft.Xna.Framework;

namespace Multiplayer_Snake.Views.Menus;

public class NameChangeView : Menu
{
    private TextInput mNameInput;
    private string nameError = "";

    public override void initializeSession()
    {
        base.initializeSession();
        var textSize = mFont.MeasureString("____________");
        mNameInput = new TextInput(mKeyboardInput, mMouseInput, 
            mGraphics.PreferredBackBufferWidth / 2, mGraphics.PreferredBackBufferHeight / 2, 
            (int)textSize.X, (int)textSize.Y, 
            mButtonBackground, mSpriteBatch, mFont,
            n =>
            {
                mMenuSelectSound.Play();
                this.submitName(n);
            },
            Client.Client.playerName);

        mSelected = null;
        var next = new MenuOption(mGame.tutorialCompleted? "Continue to Game" : "Next", () =>
        {
            if (Client.Client.playerName == "")
            {
                nameError = "Please choose a name!";
                return;
            }

            if (mGame.tutorialCompleted) mGame.changeState(GameStates.GAMEPLAY);
            else mGame.changeState(GameStates.TUTORIAL);
        }, 3 * mGraphics.PreferredBackBufferWidth / 4, 3 * mGraphics.PreferredBackBufferHeight / 4, mFont);
        
        var back = new MenuOption("Back", () =>
        {
            mGame.changeState(GameStates.MAIN_MENU);
        }, mGraphics.PreferredBackBufferWidth / 4, 3 * mGraphics.PreferredBackBufferHeight / 4, mFont);

        var submitName = new MenuOption("Submit", () => this.submitName(mNameInput.input),
            (int)(mGraphics.PreferredBackBufferWidth / 2f + textSize.X), mGraphics.PreferredBackBufferHeight / 2, mFont);
        
        registerHoverRegion(next);
        registerHoverRegion(back);
        registerHoverRegion(submitName);
        
        mMouseInput.registerMouseRegion(mNameInput.getRectangle(), MouseInput.MouseActions.L_CLICK, _ => mNameInput.focus());
        mMouseInput.registerMouseRegion(mNameInput.getRectangle(), MouseInput.MouseActions.L_CLICK, _ => mNameInput.unFocus(), null, null, true);

        mOptions = new List<MenuOption>
        {
            next,
            back,
            submitName
        };
        mDefault = next;
    }

    public override void update(GameTime gameTime)
    {
        mKeyboardInput.update(gameTime.ElapsedGameTime, true);
        if (mGame.IsActive) mMouseInput.update(gameTime.ElapsedGameTime);
        mNameInput.update(gameTime);
        mKeyboardInput.endUpdate();
    }

    private void submitName(string name)
    {
        if (name.Length < 1)
        {
            nameError = "Please choose a name!";
            return;
        }

        nameError = "";
        Client.Client.playerName = name;
    }

    public override void render(GameTime gameTime)
    {
        base.render(gameTime);
        mNameInput.render(gameTime);
        mSpriteBatch.Begin();
        if (nameError != "")
        {
            var size = mFont.MeasureString(nameError);
            mSpriteBatch.DrawString(mFont, nameError, new Vector2((mGraphics.PreferredBackBufferWidth - size.X) / 2, mGraphics.PreferredBackBufferHeight / 3f), Color.Red);
        }
        mSpriteBatch.End();
    }
}