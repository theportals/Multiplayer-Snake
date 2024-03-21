using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Multiplayer_Snake.Views.Menus;

public class MainMenuView : Menu
{
    public override void loadContent(ContentManager contentManager)
    {
        mOptions = new List<MenuOption>
        {
            // new MenuOption()
        };
    }
}