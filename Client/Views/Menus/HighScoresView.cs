using System;
using System.Collections.Generic;
using Client.Input;
using Client.Views.Menus;
using Microsoft.Xna.Framework;

namespace Client.Views;

public class HighScoresView : Menu {

    public override void initializeSession()
    {
        base.initializeSession();
        mSelected = null;
        var back = new MenuOption("Return", () => mGame.changeState(GameStates.MAIN_MENU),
            mGraphics.PreferredBackBufferWidth / 2 - 150, 2 * mGraphics.PreferredBackBufferHeight / 3, mFont);
        var reset = new MenuOption("Reset Scores", () => mGame.mHighscores.Clear(),
            mGraphics.PreferredBackBufferWidth / 2 + 150, 2 * mGraphics.PreferredBackBufferHeight / 3, mFont);
        mDefault = back;
        
        back.linkRight(reset);
        
        registerHoverRegion(back);
        registerHoverRegion(reset);

        mOptions = new List<MenuOption>
        {
            back,
            reset
        };
    }

    public override void render(GameTime gameTime)
    {
        base.render(gameTime);
        mSpriteBatch.Begin();
        var title = "High Scores";
        var size = mFont.MeasureString(title);
        mSpriteBatch.DrawString(mFont, title, new Vector2((mGraphics.PreferredBackBufferWidth - size.X) / 2, mGraphics.PreferredBackBufferHeight / 5f), Color.White);
        var x_base = mGraphics.PreferredBackBufferWidth / 2 - mFont.MeasureString("Score:       Date:      ").X / 2;
        var y_base = (mGraphics.PreferredBackBufferHeight - mFont.LineSpacing * (mGame.mHighscores.Count + 3)) / 2;
        if (mGame.mHighscores.Count == 0)
        {
            mSpriteBatch.DrawString(mFont, "No high scores saved!", new Vector2(x_base, y_base), Color.White);
            mSpriteBatch.End();
            return;
        }
        var h = mFont.MeasureString("Score:").X;
        mSpriteBatch.DrawString(mFont, "Score:", new Vector2(x_base, y_base), Color.White);
        mSpriteBatch.DrawString(mFont, "Date:", new Vector2(x_base + 300, y_base), Color.White);
        
        for (var i = 0; i < mGame.mHighscores.Count; i++)
        {
            var entry = mGame.mHighscores[i];
            var s = $"{entry.Item1}";
            var l = mFont.MeasureString(s).X;
            mSpriteBatch.DrawString(mFont, $"{entry.Item1}", new Vector2(x_base + (h - l)/2, y_base + 50 + 50 * i), Color.White);
            mSpriteBatch.DrawString(mFont, entry.Item2.ToShortDateString(), new Vector2(x_base + 300, y_base + 50 + 50 * i), Color.White);
        }
        
        mSpriteBatch.End();
    }
}