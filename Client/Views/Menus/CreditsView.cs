using System.Collections.Generic;
using Client.Input;
using Client.Views.Menus;

namespace Client.Views;

public class CreditsView : Menu
{
    public override void initializeSession()
    {
        base.initializeSession();
        mSelected = null;
        var back = new MenuOption("Return", () => mGame.changeState(GameStates.MAIN_MENU),
            mGraphics.PreferredBackBufferWidth / 2, mGraphics.PreferredBackBufferHeight / 2, mFont);
        mDefault = back;
        
        mMouseInput.registerMouseRegion(back.getRectangle(), MouseInput.MouseActions.HOVER, _ => mSelected = back, _ => mSelected = back, _ => mSelected = null);
        mMouseInput.registerMouseRegion(null, MouseInput.MouseActions.L_CLICK, null, null, _ => mSelected?.OnSelect());
        
        mOptions = new List<MenuOption>
        {
            back,
        };
    }
}