using System;
using System.Collections.Generic;
using System.Linq;
using Client.Input;
using Client.Systems;
using Client.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Client.Views.Menus;

public class ControlsView : Menu
{
    private static bool mListening;
    private static bool mFirstListeningFrame;
    private static InputDevice.Commands mRebinding;
    public override void initializeSession()
    {
        base.initializeSession();
        mSelected = null;
        var back = new MenuOption("Return", () =>
            {
                StorageUtil.storeData("keybinds.json", mKeyboardInput);
                mGame.changeState(GameStates.MAIN_MENU);
            },
            mGraphics.PreferredBackBufferWidth / 2 - 200, 3 * mGraphics.PreferredBackBufferHeight / 4, mFont);
        var reset = new MenuOption("Reset to Defaults", resetBindings,
            mGraphics.PreferredBackBufferWidth / 2 + 200, 3 * mGraphics.PreferredBackBufferHeight / 4, mFont);
        var toggleMouse = new MenuOption("Toggle", () => mKeyboardInput.listenKeys = !mKeyboardInput.listenKeys,
            3 * mGraphics.PreferredBackBufferWidth / 4, mGraphics.PreferredBackBufferHeight / 4, mFont);
        var rbUp = new MenuOption("Change", () => listenForBinding(InputDevice.Commands.UP),
            2 * mGraphics.PreferredBackBufferWidth / 3, 22 * mGraphics.PreferredBackBufferHeight / 64, mFont);
        var rbDown = new MenuOption("Change", () => listenForBinding(InputDevice.Commands.DOWN),
            2 * mGraphics.PreferredBackBufferWidth / 3, 27 * mGraphics.PreferredBackBufferHeight / 64, mFont);
        var rbLeft = new MenuOption("Change", () => listenForBinding(InputDevice.Commands.LEFT),
            2 * mGraphics.PreferredBackBufferWidth / 3, 32 * mGraphics.PreferredBackBufferHeight / 64, mFont);
        var rbRight = new MenuOption("Change", () => listenForBinding(InputDevice.Commands.RIGHT),
            2 * mGraphics.PreferredBackBufferWidth / 3, 37 * mGraphics.PreferredBackBufferHeight / 64, mFont);
        var rbBoost = new MenuOption("Change", () => listenForBinding(InputDevice.Commands.BOOST),
            2 * mGraphics.PreferredBackBufferWidth / 3, 42 * mGraphics.PreferredBackBufferHeight / 64, mFont);
        mDefault = back;
        
        registerHoverRegion(back);
        registerHoverRegion(reset);
        registerHoverRegion(toggleMouse);
        registerHoverRegion(rbUp);
        registerHoverRegion(rbDown);
        registerHoverRegion(rbLeft);
        registerHoverRegion(rbRight);
        registerHoverRegion(rbBoost);
        
        mOptions = new List<MenuOption>
        {
            back,
            reset,
            toggleMouse,
            rbUp,
            rbDown,
            rbLeft,
            rbRight,
            rbBoost
        };
    }

    private static void listenForBinding(InputDevice.Commands command)
    {
        if (!mListening) mFirstListeningFrame = true;
        mListening = true;
        mRebinding = command;
    }

    public override void update(GameTime gameTime)
    {
        mKeyboardInput.update(gameTime.ElapsedGameTime, true);
        if (mGame.IsActive && !mListening) mMouseInput.update(gameTime.ElapsedGameTime);
        if (mListening)
        {
            if (mFirstListeningFrame)
            {
                mFirstListeningFrame = false;
                return;
            }

            var np = mKeyboardInput.getNewPositiveEdges();
            if (np.Count > 0)
            {
                var k = np.First();
                if (!k.Equals(Keys.Escape))
                {
                    mKeyboardInput.bindKey(k, mRebinding);
                }

                mListening = false;
            }
        }
        mKeyboardInput.endUpdate();
    }

    private void resetBindings()
    {
        mKeyboardInput.bindKey(Keys.Up, InputDevice.Commands.UP);
        mKeyboardInput.bindKey(Keys.Down, InputDevice.Commands.DOWN);
        mKeyboardInput.bindKey(Keys.Left, InputDevice.Commands.LEFT);
        mKeyboardInput.bindKey(Keys.Right, InputDevice.Commands.RIGHT);
        mKeyboardInput.bindKey(Keys.Enter, InputDevice.Commands.SELECT);
        mKeyboardInput.bindKey(Keys.Escape, InputDevice.Commands.BACK);
        mKeyboardInput.bindKey(Keys.Space, InputDevice.Commands.BOOST);
    }

    public override void render(GameTime gameTime)
    {
        base.render(gameTime);
        mSpriteBatch.Begin();
        var scheme = "Control scheme set to: ";
        if (mKeyboardInput.listenKeys) scheme += "Keyboard";
        else scheme += "Mouse";
        var schemeSize = mFont.MeasureString(scheme);
        mSpriteBatch.DrawString(mFont, scheme, new Vector2(mGraphics.PreferredBackBufferWidth / 2f - schemeSize.X / 2,
            mGraphics.PreferredBackBufferHeight / 4f - schemeSize.Y / 2), Color.White);

        var binds = new List<string>
        {
            $"Up: {mKeyboardInput.getKey(InputDevice.Commands.UP)}",
            $"Down: {mKeyboardInput.getKey(InputDevice.Commands.DOWN)}",
            $"Left: {mKeyboardInput.getKey(InputDevice.Commands.LEFT)}",
            $"Right: {mKeyboardInput.getKey(InputDevice.Commands.RIGHT)}",
            $"Boost: {mKeyboardInput.getKey(InputDevice.Commands.BOOST)}"
        };

        for (var i = 0; i < binds.Count; i++)
        {
            var bind = binds[i];
            var size = mFont.MeasureString(bind);
            mSpriteBatch.DrawString(mFont, bind, new Vector2(mGraphics.PreferredBackBufferWidth / 2f - size.X / 2, (5 * i + 22) * (mGraphics.PreferredBackBufferHeight / 64f) - size.Y / 2), Color.White);
        }

        if (mListening)
        {
            var rebindingTitle = mRebinding.ToString();
            DrawUtil.DrawGrayOverlay(mSpriteBatch, 0.75f);
            DrawUtil.DrawStringsCentered(new List<string> {$"Press the key you'd like to bind to {rebindingTitle}", $"(or press {mKeyboardInput.getKey(InputDevice.Commands.BACK)} to cancel)"}, mFont, Color.White, mSpriteBatch);
        }
        mSpriteBatch.End();
    }
}