using System.Collections.Generic;
using Client;
using Client.Input;
using Client.Util;
using Client.Views;
using Client.Views.Menus;
using Microsoft.Xna.Framework;
using Shared.Messages;

namespace Multiplayer_Snake.Views.Menus;

public class PauseMenu : Menu
{
    public bool isOpen;
    public bool respawnChosen;
    public bool gameOver = false;
    private Dictionary<InputDevice.Commands, InputDevice.CommandEntry> mOtherBinds = new();
    private Dictionary<MouseInput.MouseRegion, InputDevice.CommandEntry> mOtherRegions = new();
    private const int dasDelay = 500;
    private const int dasPeriod = 75;
    private DASTimer t = new(dasDelay, dasPeriod);

    private GameModel mModel;

    public PauseMenu(GameModel model)
    {
        mModel = model;
        drawBackground = false;
    }

    public void open()
    {
        if (isOpen) return;
        isOpen = true;
        mOtherBinds.Clear();
        mOtherRegions.Clear();
        foreach (var entry in mKeyboardInput.mCommandEntries)
        {
            mOtherBinds.Add(entry.Key, entry.Value);
        }

        foreach (var entry in mMouseInput.mMouseRegions)
        {
            mOtherRegions.Add(entry.Key, entry.Value);
        }
        mKeyboardInput.clearCommands();
        mMouseInput.clearRegions();
        
        mKeyboardInput.registerCommand(InputDevice.Commands.BACK, _ => close());
        foreach (var option in mOptions)
        {
            registerHoverRegion(option);
        }

        mSelected = null;
        
        mKeyboardInput.registerCommand(InputDevice.Commands.UP, _ => moveUp(), gt => t.tick(gt, moveUp), t.resetTimer);
        mKeyboardInput.registerCommand(InputDevice.Commands.DOWN, _ => moveDown(), gt => t.tick(gt, moveDown), t.resetTimer);
        mKeyboardInput.registerCommand(InputDevice.Commands.LEFT, _ => moveLeft(), gt => t.tick(gt, moveLeft), t.resetTimer);
        mKeyboardInput.registerCommand(InputDevice.Commands.RIGHT, _ => moveRight(), gt => t.tick(gt, moveRight), t.resetTimer);
        mKeyboardInput.registerCommand(InputDevice.Commands.SELECT, _ =>
        {
            if (mSelected != null)
            {
                mSelected.OnSelect();
                mMenuSelectSound.Play();
            }
        });
        
        mMouseInput.registerMouseRegion(null, MouseInput.MouseActions.L_CLICK, null, null, _ =>
        {
            if (mSelected != null)
            {
                mSelected.OnSelect();
                mMenuSelectSound.Play();
            }
        });
    }

    public void close()
    {
        if (!isOpen) return;
        isOpen = false;
        mKeyboardInput.clearCommands();
        mMouseInput.clearRegions();
        foreach (var entry in mOtherBinds)
        {
            mKeyboardInput.mCommandEntries.Add(entry.Key, entry.Value);
        }
        foreach (var entry in mOtherRegions)
        {
            mMouseInput.mMouseRegions.Add(entry.Key, entry.Value);
        }
    }

    public void toggle()
    {
        if (isOpen) close();
        else open();
    }

    public override void initializeSession()
    {
        var close = new MenuOption("Close Menu", this.close, mGraphics.PreferredBackBufferWidth / 3, 2 * mGraphics.PreferredBackBufferHeight / 3, mFont);
        var mainMenu = new MenuOption("Return to Title", () =>
        {
            mModel.disconnect();
        }, 2 * mGraphics.PreferredBackBufferWidth / 3, 2 * mGraphics.PreferredBackBufferHeight / 3, mFont);
        var respawn = new MenuOption("Respawn", () =>
        {
            this.close();
            // mModel.bindBoost();
            MessageQueueClient.instance.sendMessageWithId(new Respawn(Client.Client.playerName));
            gameOver = false;
        }, mGraphics.PreferredBackBufferWidth / 2, mGraphics.PreferredBackBufferHeight / 2, mFont);
        mSelected = null;
        mDefault = close;
        
        close.linkRight(mainMenu);
        close.linkUp(respawn);
        mainMenu.linkUp(respawn, false);
        respawn.linkRight(mainMenu, false);
        respawn.linkLeft(mainMenu, false);
        
        mOptions = new List<MenuOption>
        {
            close,
            mainMenu,
            respawn
        };
    }

    public override void render(GameTime gameTime)
    {
        if (!isOpen) return;
        mSpriteBatch.Begin();
        DrawUtil.DrawGrayOverlay(mSpriteBatch);
        mSpriteBatch.End();
        base.render(gameTime);
        if (!gameOver) return;
        mSpriteBatch.Begin();

        var strings = new List<string>()
        {
            "Game Over!",
            $"Your Score: {mModel.mScore}",
            $"Kills: {mModel.mKills}",
            $"Best Rank: #{mModel.mBestRank} / {mModel.mLeaderboard.Count + 1}",
        };
        for (var i = 0; i < strings.Count; i++)
        {
            var s = strings[i];
            var size = mFont.MeasureString(s);
            mSpriteBatch.DrawString(mFont, s,
                new Vector2((mGraphics.PreferredBackBufferWidth - size.X) / 2,
                    mGraphics.PreferredBackBufferHeight / 4f - size.Y + mFont.LineSpacing * i), Color.White);
        }

        mSpriteBatch.End();
    }
}