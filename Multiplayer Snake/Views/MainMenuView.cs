using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Multiplayer_Snake.Views;

public class MainMenuView : GameStateView
{
    public override void loadContent(ContentManager contentManager)
    {
    }

    public override GameStates processInput(GameTime gameTime)
    {
        return GameStates.MAIN_MENU;
    }

    public override void update(GameTime gameTime)
    {
    }

    public override void render(GameTime gameTime)
    {
    }
}