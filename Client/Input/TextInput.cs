using System;
using System.Collections.Generic;
using Client.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Client.Input;

public class TextInput
{
    private KeyboardInput mKeyboardInput;
    private MouseInput mMouseInput;
    private Dictionary<InputDevice.Commands, InputDevice.CommandEntry> mOtherBinds = new();
    private SpriteFont mFont;
    private Rectangle mRec;
    private Action<string> onSubmit;

    private const int MAX_LENGTH = 12;

    private Texture2D mBackground;
    private SpriteBatch mSpriteBatch;
    
    private bool mFocused = false;
    public string input;

    private double focusTime = 0;

    public TextInput(KeyboardInput keyboardInput, MouseInput mouseInput, int x, int y, int width, int height, Texture2D background, SpriteBatch spriteBatch, SpriteFont font, Action<string> onSubmit, string firstInput
    )
    {
        mKeyboardInput = keyboardInput;
        mMouseInput = mouseInput;
        const int padding = 25;
        mRec = new Rectangle(x - (width + padding * 2) / 2, y - height / 2, width + padding * 2, height);
        mBackground = background;
        mSpriteBatch = spriteBatch;
        mFont = font;
        this.onSubmit = onSubmit;
        this.input = firstInput;
    }

    public void focus()
    {
        if (mFocused) return;
        mFocused = true;
        mOtherBinds.Clear();
        foreach (var entry in mKeyboardInput.mCommandEntries)
        {
            mOtherBinds.Add(entry.Key, entry.Value);
        }
        mKeyboardInput.clearCommands();
        mKeyboardInput.registerCommand(InputDevice.Commands.BACK, _ => unFocus());
        mKeyboardInput.registerCommand(InputDevice.Commands.SELECT, _ => submit());
    }

    public void submit()
    {
        onSubmit.Invoke(input);
        unFocus();
    }

    public void unFocus()
    {
        if (!mFocused) return;
        mFocused = false;
        mKeyboardInput.clearCommands();
        foreach (var entry in mOtherBinds)
        {
            mKeyboardInput.mCommandEntries.Add(entry.Key, entry.Value);
        }

        focusTime = 0;
    }

    public Rectangle getRectangle()
    {
        return mRec;
    }

    public void update(GameTime gameTime)
    {
        if (!mFocused) return;
        focusTime += gameTime.ElapsedGameTime.TotalSeconds;

        var pe = mKeyboardInput.getNewPositiveEdges();
        foreach (var key in pe)
        {
            if (key == Keys.Back)
            {
                if (input.Length < 1) continue;
                input = input.Remove(input.Length - 1);
                continue;
            }
            if (input.Length < MAX_LENGTH) input += TextInputUtil.getCharacter(key);
            // if (input.Length < MAX_LENGTH) input += key.ToString();
        }
    }

    public void render(GameTime gameTime)
    {
        mSpriteBatch.Begin();
        mSpriteBatch.Draw(mBackground, mRec, Color.White);
        var s = input;
        var textSize = mFont.MeasureString(s);
        var c = Color.Black;
        if (input.Length == 0)
        {
            s = "Input name...";
            c = Color.Gray;
        }
        mSpriteBatch.DrawString(mFont, s, new Vector2(mRec.X + 25, mRec.Y), c);

        if (mFocused)
        {
            var cursor = "|";
            if (Math.Floor(focusTime * 2) % 2 == 0) mSpriteBatch.DrawString(mFont, cursor, new Vector2(mRec.X + 25 + textSize.X, mRec.Y), Color.Black);
        }
        mSpriteBatch.End();
    }
    
}