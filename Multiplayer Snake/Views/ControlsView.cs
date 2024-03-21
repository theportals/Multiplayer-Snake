using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Multiplayer_Snake.Views;

public class ControlsView : GameStateView
{
    public override void loadContent(ContentManager contentManager)
    {
    }

    public override GameStates processInput(GameTime gameTime)
    {
        return GameStates.CONTROLS;
    }

    public override void update(GameTime gameTime)
    {
    }

    public override void render(GameTime gameTime)
    {
    }
}