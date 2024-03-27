using System;
using System.Collections.Generic;
using Client.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Views.Menus;

public class MainMenuView : Menu
{
    public override void initializeSession()
    {
        base.initializeSession();
        mSelected = null;
        var start = 2 * mGraphics.PreferredBackBufferHeight / 3;
        const int spacing = 100;
        var newGame = new MenuOption("New Game", () => mGame.changeState(GameStates.GAMEPLAY), mGraphics.PreferredBackBufferWidth / 3, start, mFont);
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
        
        mMouseInput.registerMouseRegion(newGame.getRectangle(), MouseInput.MouseActions.HOVER, _ => mSelected = newGame, _ => mSelected = newGame, _ => mSelected = null);
        mMouseInput.registerMouseRegion(highScores.getRectangle(), MouseInput.MouseActions.HOVER, _ => mSelected = highScores, _ => mSelected = highScores, _ => mSelected = null);
        mMouseInput.registerMouseRegion(controls.getRectangle(), MouseInput.MouseActions.HOVER, _ => mSelected = controls, _ => mSelected = controls, _ => mSelected = null);
        mMouseInput.registerMouseRegion(credits.getRectangle(), MouseInput.MouseActions.HOVER, _ => mSelected = credits, _ => mSelected = credits, _ => mSelected = null);
        mMouseInput.registerMouseRegion(exit.getRectangle(), MouseInput.MouseActions.HOVER, _ => mSelected = exit, _ => mSelected = exit, _ => mSelected = null);
        mMouseInput.registerMouseRegion(null, MouseInput.MouseActions.L_CLICK, null, null, _ => mSelected?.OnSelect());
        
        
        mOptions = new List<MenuOption>
        {
            newGame,
            highScores,
            controls,
            credits,
            exit
        };
        mDefault = newGame;
    }
}